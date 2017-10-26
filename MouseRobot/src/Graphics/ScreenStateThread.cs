using Robot.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Robot.Graphics
{
    public class ScreenStateThread : StableRepeatingThread
    {
        public object ScreenBmpLock = new object();
        /// <summary>
        /// Lock before using it, since data is constantly being written to this bitmap
        /// </summary>
        public Bitmap ScreenBmp
        {
            get
            {
                return m_ScreenBmp;
            }
            private set
            {
                m_ScreenBmp = value;
            }
        }
        private Bitmap m_ScreenBmp;
        private Bitmap m_TempBitmap;

        private Device m_Device;
        private Output1 m_Output1;
        private Texture2D m_ScreenTexture;
        private OutputDuplication m_DuplicatedOutput;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public static ScreenStateThread Instace { get { return m_Instance; } }
        private static ScreenStateThread m_Instance = new ScreenStateThread();
        private ScreenStateThread() { }

        public override void Init()
        {
            base.Init();

            var adapter = new Factory1().GetAdapter1(0);
            m_Device = new SharpDX.Direct3D11.Device(adapter);

            var output = adapter.GetOutput(0);

            m_Output1 = output.QueryInterface<Output1>();
            Width = output.Description.DesktopBounds.Right;
            Height = output.Description.DesktopBounds.Bottom;

            m_DuplicatedOutput = m_Output1.DuplicateOutput(m_Device);

            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = Width,
                Height = Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            m_ScreenTexture = new Texture2D(m_Device, textureDesc);

            m_ScreenBmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            m_TempBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        }

        protected override void ThreadAction()
        {
            TakeScreenshot(m_TempBitmap);

            lock (ScreenBmpLock)
            {
                BitmapUtility.Clone32BPPBitmap(m_TempBitmap, ScreenBmp);
            }
        }

        public void TakeScreenshot(Bitmap tempBitmap)
        {
            try
            {
                SharpDX.DXGI.Resource screenResource;
                OutputDuplicateFrameInformation duplicateFrameInformation;

                // Try to get duplicated frame within given time is ms
                m_DuplicatedOutput.AcquireNextFrame(20, out duplicateFrameInformation, out screenResource);

                // copy resource into memory that can be accessed by the CPU
                using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    m_Device.ImmediateContext.CopyResource(screenTexture2D, m_ScreenTexture);

                // Get the desktop capture texture
                var mapSource = m_Device.ImmediateContext.MapSubresource(m_ScreenTexture, 0, MapMode.Read, MapFlags.None);

                // using bitmap was here
                var boundsRect = new Rectangle(0, 0, Width, Height);

                // Copy pixels from screen capture Texture to GDI bitmap
                var mapDest = tempBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, tempBitmap.PixelFormat);
                var sourcePtr = mapSource.DataPointer;
                var destPtr = mapDest.Scan0;
                for (int y = 0; y < Height; y++)
                {
                    // Copy a single line 
                    Utilities.CopyMemory(destPtr, sourcePtr, Width * 4);

                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }

                // Release source and dest locks
                tempBitmap.UnlockBits(mapDest);

                // Release memory
                m_Device.ImmediateContext.UnmapSubresource(m_ScreenTexture, 0);
                screenResource.Dispose();
                m_DuplicatedOutput.ReleaseFrame();
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    Trace.TraceError(e.Message);
                    Trace.TraceError(e.StackTrace);
                }
            }
        }
    }
}
