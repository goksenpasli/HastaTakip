using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace Webp
{
    public sealed class WebP : IDisposable
    {
        #region | Destruction |

        public void Dispose() => GC.SuppressFinalize(this);

        #endregion | Destruction |

        #region | Public Decompress Functions |

        public Bitmap Load(string pathFileName)
        {
            try
            {
                var rawWebP = File.ReadAllBytes(pathFileName);

                return Decode(rawWebP);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Load");
            }
        }

        public Bitmap Decode(byte[] rawWebP)
        {
            int outputSize;
            Bitmap bmp = null;
            BitmapData bmpData = null;
            var pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);

            try
            {
                var ptrData = pinnedWebP.AddrOfPinnedObject();
                if (UnsafeNativeMethods.WebPGetInfo(ptrData, rawWebP.Length, out var imgWidth, out var imgHeight) == 0)
                    throw new Exception("Can´t get information of WebP");

                bmp = new Bitmap(imgWidth, imgHeight, PixelFormat.Format24bppRgb);
                bmpData = bmp.LockBits(new Rectangle(0, 0, imgWidth, imgHeight), ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                outputSize = bmpData.Stride * imgHeight;
                if (UnsafeNativeMethods.WebPDecodeBGRInto(ptrData, rawWebP.Length, bmpData.Scan0, outputSize,
                        bmpData.Stride) == 0)
                    throw new Exception("Can´t decode WebP");

                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Decode");
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (pinnedWebP.IsAllocated)
                    pinnedWebP.Free();
            }
        }

        public Bitmap Decode(byte[] rawWebP, WebPDecoderOptions options)
        {
            var pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            Bitmap bmp = null;
            BitmapData bmpData = null;
            VP8StatusCode result;
            var width = 0;
            var height = 0;

            try
            {
                var config = new WebPDecoderConfig();
                if (UnsafeNativeMethods.WebPInitDecoderConfig(ref config) == 0)
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");

                var ptrRawWebP = pinnedWebP.AddrOfPinnedObject();
                if (options.use_scaling == 0)
                {
                    result = UnsafeNativeMethods.WebPGetFeatures(ptrRawWebP, rawWebP.Length, ref config.input);
                    if (result != VP8StatusCode.VP8_STATUS_OK)
                        throw new Exception("Failed WebPGetFeatures with error " + result);

                    if (options.use_cropping == 1)
                    {
                        if (options.crop_left + options.crop_width > config.input.width ||
                            options.crop_top + options.crop_height > config.input.height)
                            throw new Exception("Crop options exceded WebP image dimensions");
                        width = options.crop_width;
                        height = options.crop_height;
                    }
                }
                else
                {
                    width = options.scaled_width;
                    height = options.scaled_height;
                }

                config.options.bypass_filtering = options.bypass_filtering;
                config.options.no_fancy_upsampling = options.no_fancy_upsampling;
                config.options.use_cropping = options.use_cropping;
                config.options.crop_left = options.crop_left;
                config.options.crop_top = options.crop_top;
                config.options.crop_width = options.crop_width;
                config.options.crop_height = options.crop_height;
                config.options.use_scaling = options.use_scaling;
                config.options.scaled_width = options.scaled_width;
                config.options.scaled_height = options.scaled_height;
                config.options.use_threads = options.use_threads;
                config.options.dithering_strength = options.dithering_strength;
                config.options.flip = options.flip;
                config.options.alpha_dithering_strength = options.alpha_dithering_strength;

                bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                config.output.colorspace = WEBP_CSP_MODE.MODE_BGR;
                config.output.u.RGBA.rgba = bmpData.Scan0;
                config.output.u.RGBA.stride = bmpData.Stride;
                config.output.u.RGBA.size = (UIntPtr)(bmp.Height * bmpData.Stride);
                config.output.height = bmp.Height;
                config.output.width = bmp.Width;
                config.output.is_external_memory = 1;

                result = UnsafeNativeMethods.WebPDecode(ptrRawWebP, rawWebP.Length, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                    throw new Exception("Failed WebPDecode with error " + result);
                UnsafeNativeMethods.WebPFreeDecBuffer(ref config.output);

                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Decode");
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (pinnedWebP.IsAllocated)
                    pinnedWebP.Free();
            }
        }

        public Bitmap Thumbnail(byte[] rawWebP, int width, int height)
        {
            var pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            Bitmap bmp = null;
            BitmapData bmpData = null;

            try
            {
                var config = new WebPDecoderConfig();
                if (UnsafeNativeMethods.WebPInitDecoderConfig(ref config) == 0)
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");

                config.options.bypass_filtering = 1;
                config.options.no_fancy_upsampling = 1;
                config.options.use_threads = 1;
                config.options.use_scaling = 1;
                config.options.scaled_width = width;
                config.options.scaled_height = height;

                bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                config.output.colorspace = WEBP_CSP_MODE.MODE_BGR;
                config.output.u.RGBA.rgba = bmpData.Scan0;
                config.output.u.RGBA.stride = bmpData.Stride;
                config.output.u.RGBA.size = (UIntPtr)(bmp.Height * bmpData.Stride);
                config.output.height = bmp.Height;
                config.output.width = bmp.Width;
                config.output.is_external_memory = 1;

                var ptrRawWebP = pinnedWebP.AddrOfPinnedObject();
                var result = UnsafeNativeMethods.WebPDecode(ptrRawWebP, rawWebP.Length, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                    throw new Exception("Failed WebPDecode with error " + result);

                UnsafeNativeMethods.WebPFreeDecBuffer(ref config.output);

                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Thumbnail");
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (pinnedWebP.IsAllocated)
                    pinnedWebP.Free();
            }
        }

        #endregion | Public Decompress Functions |

        #region | Public Compress Functions |

        public void Save(Bitmap bmp, string pathFileName, int quality = 75)
        {
            byte[] rawWebP;

            try
            {
                rawWebP = EncodeLossy(bmp, quality);

                File.WriteAllBytes(pathFileName, rawWebP);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Save");
            }
        }

        public byte[] EncodeLossy(Bitmap bmp, int quality = 75)
        {
            BitmapData bmpData = null;
            var unmanagedData = IntPtr.Zero;
            byte[] rawWebP = null;

            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);

                var size = UnsafeNativeMethods.WebPEncodeBGR(bmpData.Scan0, bmp.Width, bmp.Height, bmpData.Stride,
                    quality, out unmanagedData);
                if (size == 0)
                    throw new Exception("Can´t encode WebP");

                rawWebP = new byte[size];
                Marshal.Copy(unmanagedData, rawWebP, 0, size);

                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossly");
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (unmanagedData != IntPtr.Zero)
                    UnsafeNativeMethods.WebPFree(unmanagedData);
            }
        }

        public byte[] EncodeLossy(Bitmap bmp, int quality, int speed, bool info = false)
        {
            byte[] rawWebP = null;
            var wpic = new WebPPicture();
            BitmapData bmpData = null;
            var stats = new WebPAuxStats();
            var ptrStats = IntPtr.Zero;

            try
            {
                var config = new WebPConfig();

                if (UnsafeNativeMethods.WebPConfigInit(ref config, WebPPreset.WEBP_PRESET_DEFAULT, 75) == 0)
                    throw new Exception("Can´t config preset");

                config.method = speed;
                if (config.method > 6)
                    config.method = 6;
                config.quality = quality;
                config.autofilter = 1;
                config.pass = speed + 1;
                config.segments = 4;
                config.partitions = 3;
                config.thread_level = 1;
                config.preprocessing = 4;

                if (UnsafeNativeMethods.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");
                wpic.width = bmp.Width;
                wpic.height = bmp.Height;
                wpic.use_argb = 1;

                if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, bmpData.Scan0, bmpData.Stride) != 1)
                    throw new Exception("Can´t allocate memory in WebPPictureImportBGR");

                if (info)
                {
                    stats = new WebPAuxStats();
                    ptrStats = Marshal.AllocHGlobal(Marshal.SizeOf(stats));
                    Marshal.StructureToPtr(stats, ptrStats, false);
                    wpic.stats = ptrStats;
                }

                webpMemory = new MemoryWriter
                {
                    data = new byte[bmp.Width * bmp.Height * 24],
                    size = 0
                };
                UnsafeNativeMethods.OnCallback = MyWriter;
                wpic.writer = Marshal.GetFunctionPointerForDelegate(UnsafeNativeMethods.OnCallback);

                if (UnsafeNativeMethods.WebPEncode(ref config, ref wpic) != 1)
                    throw new Exception("Encoding error: " + (WebPEncodingError)wpic.error_code);

                bmp.UnlockBits(bmpData);
                bmpData = null;

                rawWebP = new byte[webpMemory.size];
                Array.Copy(webpMemory.data, rawWebP, webpMemory.size);

                if (info)
                {
                    stats = (WebPAuxStats)Marshal.PtrToStructure(ptrStats, typeof(WebPAuxStats));
                    MessageBox.Show("Dimension: " + wpic.width + " x " + wpic.height + " pixels\n" +
                                    "Output:    " + stats.coded_size + " bytes\n" +
                                    "PSNR Y:    " + stats.PSNRY + " db\n" +
                                    "PSNR u:    " + stats.PSNRU + " db\n" +
                                    "PSNR v:    " + stats.PSNRV + " db\n" +
                                    "PSNR ALL:  " + stats.PSNRALL + " db\n" +
                                    "Block intra4:  " + stats.block_count_intra4 + "\n" +
                                    "Block intra16: " + stats.block_count_intra16 + "\n" +
                                    "Block skipped: " + stats.block_count_skipped + "\n" +
                                    "Header size:    " + stats.header_bytes + " bytes\n" +
                                    "Mode-partition: " + stats.mode_partition_0 + " bytes\n" +
                                    "Macroblocks 0:  " + stats.segment_size_segments0 + " residuals bytes\n" +
                                    "Macroblocks 1:  " + stats.segment_size_segments1 + " residuals bytes\n" +
                                    "Macroblocks 2:  " + stats.segment_size_segments2 + " residuals bytes\n" +
                                    "Macroblocks 3:  " + stats.segment_size_segments3 + " residuals bytes\n" +
                                    "Quantizer   0:  " + stats.segment_quant_segments0 + " residuals bytes\n" +
                                    "Quantizer   1:  " + stats.segment_quant_segments1 + " residuals bytes\n" +
                                    "Quantizer   2:  " + stats.segment_quant_segments2 + " residuals bytes\n" +
                                    "Quantizer   3:  " + stats.segment_quant_segments3 + " residuals bytes\n" +
                                    "Filter level 0: " + stats.segment_level_segments0 + " residuals bytes\n" +
                                    "Filter level 1: " + stats.segment_level_segments1 + " residuals bytes\n" +
                                    "Filter level 2: " + stats.segment_level_segments2 + " residuals bytes\n" +
                                    "Filter level 3: " + stats.segment_level_segments3 + " residuals bytes\n");
                }

                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossly (Advanced)");
            }
            finally
            {
                if (ptrStats != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrStats);

                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (wpic.argb != IntPtr.Zero)
                    UnsafeNativeMethods.WebPPictureFree(ref wpic);
            }
        }

        public byte[] EncodeLossless(Bitmap bmp)
        {
            BitmapData bmpData = null;
            var unmanagedData = IntPtr.Zero;
            byte[] rawWebP = null;

            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);

                var size = UnsafeNativeMethods.WebPEncodeLosslessBGR(bmpData.Scan0, bmp.Width, bmp.Height,
                    bmpData.Stride, out unmanagedData);

                rawWebP = new byte[size];
                Marshal.Copy(unmanagedData, rawWebP, 0, size);

                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossless (Simple)");
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (unmanagedData != IntPtr.Zero)
                    UnsafeNativeMethods.WebPFree(unmanagedData);
            }
        }

        public byte[] EncodeLossless(Bitmap bmp, int speed, bool info = false)
        {
            byte[] rawWebP = null;
            var wpic = new WebPPicture();
            BitmapData bmpData = null;
            var stats = new WebPAuxStats();
            var ptrStats = IntPtr.Zero;

            try
            {
                var config = new WebPConfig();

                if (UnsafeNativeMethods.WebPConfigInit(ref config, WebPPreset.WEBP_PRESET_DEFAULT, (speed + 1) * 10) ==
                    0)
                    throw new Exception("Can´t config preset");
                if (UnsafeNativeMethods.WebPConfigLosslessPreset(ref config, speed) == 0)
                    throw new Exception("Can´t config lossless preset");
                config.pass = speed + 1;

                if (UnsafeNativeMethods.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");
                wpic.width = bmp.Width;
                wpic.height = bmp.Height;
                wpic.use_argb = 1;

                if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, bmpData.Scan0, bmpData.Stride) != 1)
                    throw new Exception("Can´t allocate memory in WebPPictureImportBGR");

                if (info)
                {
                    stats = new WebPAuxStats();
                    ptrStats = Marshal.AllocHGlobal(Marshal.SizeOf(stats));
                    Marshal.StructureToPtr(stats, ptrStats, false);
                    wpic.stats = ptrStats;
                }

                webpMemory = new MemoryWriter
                {
                    data = new byte[bmp.Width * bmp.Height * 24],
                    size = 0
                };
                UnsafeNativeMethods.OnCallback = MyWriter;
                wpic.writer = Marshal.GetFunctionPointerForDelegate(UnsafeNativeMethods.OnCallback);

                if (UnsafeNativeMethods.WebPEncode(ref config, ref wpic) != 1)
                    throw new Exception("Encoding error: " + (WebPEncodingError)wpic.error_code);

                bmp.UnlockBits(bmpData);
                bmpData = null;

                rawWebP = new byte[webpMemory.size];
                Array.Copy(webpMemory.data, rawWebP, webpMemory.size);

                if (info)
                {
                    stats = (WebPAuxStats)Marshal.PtrToStructure(ptrStats, typeof(WebPAuxStats));
                    var features = "";
                    if ((stats.lossless_features & 1) > 0) features = " PREDICTION";
                    if ((stats.lossless_features & 2) > 0) features += " CROSS-COLOR-TRANSFORM";
                    if ((stats.lossless_features & 4) > 0) features += " SUBTRACT-GREEN";
                    if ((stats.lossless_features & 8) > 0) features += " PALETTE";
                    MessageBox.Show("Dimension: " + wpic.width + " x " + wpic.height + " pixels\n" +
                                    "Output:    " + stats.coded_size + " bytes\n" +
                                    "Losslesss compressed size: " + stats.lossless_size + " bytes\n" +
                                    "  * Header size: " + stats.lossless_hdr_size + " bytes\n" +
                                    "  * Image data size: " + stats.lossless_data_size + " bytes\n" +
                                    "  * Lossless features used:" + features + "\n" +
                                    "  * Precision Bits: histogram=" + stats.histogram_bits + " transform=" +
                                    stats.transform_bits + " cache=" + stats.cache_bits);
                }

                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossless");
            }
            finally
            {
                if (ptrStats != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrStats);

                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (wpic.argb != IntPtr.Zero)
                    UnsafeNativeMethods.WebPPictureFree(ref wpic);
            }
        }

        public byte[] EncodeNearLossless(Bitmap bmp, int quality, int speed = 9, bool info = false)
        {
            byte[] rawWebP = null;
            var wpic = new WebPPicture();
            BitmapData bmpData = null;
            var stats = new WebPAuxStats();
            var ptrStats = IntPtr.Zero;

            try
            {
                var config = new WebPConfig();

                if (UnsafeNativeMethods.WebPConfigInit(ref config, WebPPreset.WEBP_PRESET_DEFAULT, (speed + 1) * 10) ==
                    0)
                    throw new Exception("Can´t config preset");
                if (UnsafeNativeMethods.WebPConfigLosslessPreset(ref config, speed) == 0)
                    throw new Exception("Can´t config lossless preset");
                config.pass = speed + 1;
                config.near_lossless = quality;

                if (UnsafeNativeMethods.WebPValidateConfig(ref config) != 1)
                    throw new Exception("Bad config parameters");

                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);
                wpic = new WebPPicture();
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpic) != 1)
                    throw new Exception("Can´t init WebPPictureInit");
                wpic.width = bmp.Width;
                wpic.height = bmp.Height;
                wpic.use_argb = 1;

                if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, bmpData.Scan0, bmpData.Stride) != 1)
                    throw new Exception("Can´t allocate memory in WebPPictureImportBGR");

                if (info)
                {
                    stats = new WebPAuxStats();
                    ptrStats = Marshal.AllocHGlobal(Marshal.SizeOf(stats));
                    Marshal.StructureToPtr(stats, ptrStats, false);
                    wpic.stats = ptrStats;
                }

                webpMemory = new MemoryWriter
                {
                    data = new byte[bmp.Width * bmp.Height * 24],
                    size = 0
                };
                UnsafeNativeMethods.OnCallback = MyWriter;
                wpic.writer = Marshal.GetFunctionPointerForDelegate(UnsafeNativeMethods.OnCallback);

                if (UnsafeNativeMethods.WebPEncode(ref config, ref wpic) != 1)
                    throw new Exception("Encoding error: " + (WebPEncodingError)wpic.error_code);

                bmp.UnlockBits(bmpData);
                bmpData = null;

                rawWebP = new byte[webpMemory.size];
                Array.Copy(webpMemory.data, rawWebP, webpMemory.size);

                if (info)
                {
                    stats = (WebPAuxStats)Marshal.PtrToStructure(ptrStats, typeof(WebPAuxStats));
                    var features = "";
                    if ((stats.lossless_features & 1) > 0) features = " PREDICTION";
                    if ((stats.lossless_features & 2) > 0) features += " CROSS-COLOR-TRANSFORM";
                    if ((stats.lossless_features & 4) > 0) features += " SUBTRACT-GREEN";
                    if ((stats.lossless_features & 8) > 0) features += " PALETTE";
                    MessageBox.Show("Dimension: " + wpic.width + " x " + wpic.height + " pixels\n" +
                                    "Output:    " + stats.coded_size + " bytes\n" +
                                    "Losslesss compressed size: " + stats.lossless_size + " bytes\n" +
                                    "  * Header size: " + stats.lossless_hdr_size + " bytes\n" +
                                    "  * Image data size: " + stats.lossless_data_size + " bytes\n" +
                                    "  * Lossless features used:" + features + "\n" +
                                    "  * Precision Bits: histogram=" + stats.histogram_bits + " transform=" +
                                    stats.transform_bits + " cache=" + stats.cache_bits);
                }

                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.EncodeNearLossless");
            }
            finally
            {
                if (ptrStats != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrStats);

                if (bmpData != null)
                    bmp.UnlockBits(bmpData);

                if (wpic.argb != IntPtr.Zero)
                    UnsafeNativeMethods.WebPPictureFree(ref wpic);
            }
        }

        #endregion | Public Compress Functions |

        #region | Another Public Functions |

        public string GetVersion()
        {
            try
            {
                var v = (uint)UnsafeNativeMethods.WebPGetDecoderVersion();
                var revision = v % 256;
                var minor = (v >> 8) % 256;
                var major = (v >> 16) % 256;
                return major + "." + minor + "." + revision;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.GetVersion");
            }
        }

        public void GetInfo(byte[] rawWebP, out int width, out int height, out bool has_alpha, out bool has_animation,
            out string format)
        {
            VP8StatusCode result;
            var pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);

            try
            {
                var ptrRawWebP = pinnedWebP.AddrOfPinnedObject();

                var features = new WebPBitstreamFeatures();
                result = UnsafeNativeMethods.WebPGetFeatures(ptrRawWebP, rawWebP.Length, ref features);

                if (result != 0)
                    throw new Exception(result.ToString());

                width = features.width;
                height = features.height;
                has_alpha = features.has_alpha == 1;
                has_animation = features.has_animation == 1;
                switch (features.format)
                {
                    case 1:
                        format = "lossy";
                        break;

                    case 2:
                        format = "lossless";
                        break;

                    default:
                        format = "undefined";
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.GetInfo");
            }
            finally
            {
                if (pinnedWebP.IsAllocated)
                    pinnedWebP.Free();
            }
        }

        public float[] GetPictureDistortion(Bitmap source, Bitmap reference, int metric_type)
        {
            var wpicSource = new WebPPicture();
            var wpicReference = new WebPPicture();
            BitmapData sourceBmpData = null;
            BitmapData referenceBmpData = null;
            var result = new float[5];
            var pinnedResult = GCHandle.Alloc(result, GCHandleType.Pinned);

            try
            {
                if (source == null)
                    throw new Exception("Source picture is void");
                if (reference == null)
                    throw new Exception("Reference picture is void");
                if (metric_type > 2)
                    throw new Exception("Bad metric_type. Use 0 = PSNR, 1 = SSIM, 2 = LSIM");
                if (source.Width != reference.Width || source.Height != reference.Height)
                    throw new Exception("Source and Reference pictures have diferent dimensions");

                sourceBmpData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                wpicSource = new WebPPicture();
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpicSource) != 1)
                    throw new Exception("Can´t init WebPPictureInit");
                wpicSource.width = source.Width;
                wpicSource.height = source.Height;
                wpicSource.use_argb = 1;
                if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpicSource, sourceBmpData.Scan0,
                        sourceBmpData.Stride) != 1)
                    throw new Exception("Can´t allocate memory in WebPPictureImportBGR");

                referenceBmpData = reference.LockBits(new Rectangle(0, 0, reference.Width, reference.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                wpicReference = new WebPPicture();
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpicReference) != 1)
                    throw new Exception("Can´t init WebPPictureInit");
                wpicReference.width = reference.Width;
                wpicReference.height = reference.Height;
                wpicReference.use_argb = 1;
                if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpicReference, referenceBmpData.Scan0,
                        referenceBmpData.Stride) != 1)
                    throw new Exception("Can´t allocate memory in WebPPictureImportBGR");

                var ptrResult = pinnedResult.AddrOfPinnedObject();
                if (UnsafeNativeMethods.WebPPictureDistortion(ref wpicSource, ref wpicReference, metric_type,
                        ptrResult) != 1)
                    throw new Exception("Can´t measure.");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.GetPictureDistortion");
            }
            finally
            {
                if (sourceBmpData != null)
                    source.UnlockBits(sourceBmpData);
                if (referenceBmpData != null)
                    reference.UnlockBits(referenceBmpData);

                if (wpicSource.argb != IntPtr.Zero)
                    UnsafeNativeMethods.WebPPictureFree(ref wpicSource);
                if (wpicReference.argb != IntPtr.Zero)
                    UnsafeNativeMethods.WebPPictureFree(ref wpicReference);
                if (pinnedResult.IsAllocated)
                    pinnedResult.Free();
            }
        }

        #endregion | Another Public Functions |

        #region | Private Methods |

        private MemoryWriter webpMemory;

        private int MyWriter([In] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            Marshal.Copy(data, webpMemory.data, webpMemory.size, (int)data_size);
            webpMemory.size += (int)data_size;
            return 1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryWriter
        {
            public int size;
            public byte[] data;
        }

        #endregion | Private Methods |
    }

    #region | Import libwebp functions |

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int WebPMemoryWrite([In] IntPtr data, UIntPtr data_size, ref WebPPicture wpic);

        private const int WEBP_DECODER_ABI_VERSION = 0x0208;

        public static WebPMemoryWrite OnCallback;

        public static int WebPConfigInit(ref WebPConfig config, WebPPreset preset, float quality)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPConfigInitInternal_x86(ref config, preset, quality, WEBP_DECODER_ABI_VERSION);

                case 8:
                    return WebPConfigInitInternal_x64(ref config, preset, quality, WEBP_DECODER_ABI_VERSION);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPConfigInitInternal")]
        private static extern int WebPConfigInitInternal_x86(ref WebPConfig config, WebPPreset preset, float quality,
            int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPConfigInitInternal")]
        private static extern int WebPConfigInitInternal_x64(ref WebPConfig config, WebPPreset preset, float quality,
            int WEBP_DECODER_ABI_VERSION);

        public static VP8StatusCode WebPGetFeatures(IntPtr rawWebP, int data_size, ref WebPBitstreamFeatures features)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPGetFeaturesInternal_x86(rawWebP, (UIntPtr)data_size, ref features,
                        WEBP_DECODER_ABI_VERSION);

                case 8:
                    return WebPGetFeaturesInternal_x64(rawWebP, (UIntPtr)data_size, ref features,
                        WEBP_DECODER_ABI_VERSION);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPGetFeaturesInternal")]
        private static extern VP8StatusCode WebPGetFeaturesInternal_x86([In] IntPtr rawWebP, UIntPtr data_size,
            ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPGetFeaturesInternal")]
        private static extern VP8StatusCode WebPGetFeaturesInternal_x64([In] IntPtr rawWebP, UIntPtr data_size,
            ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);

        public static int WebPConfigLosslessPreset(ref WebPConfig config, int level)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPConfigLosslessPreset_x86(ref config, level);

                case 8:
                    return WebPConfigLosslessPreset_x64(ref config, level);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPConfigLosslessPreset")]
        private static extern int WebPConfigLosslessPreset_x86(ref WebPConfig config, int level);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPConfigLosslessPreset")]
        private static extern int WebPConfigLosslessPreset_x64(ref WebPConfig config, int level);

        public static int WebPValidateConfig(ref WebPConfig config)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPValidateConfig_x86(ref config);

                case 8:
                    return WebPValidateConfig_x64(ref config);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPValidateConfig")]
        private static extern int WebPValidateConfig_x86(ref WebPConfig config);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPValidateConfig")]
        private static extern int WebPValidateConfig_x64(ref WebPConfig config);

        public static int WebPPictureInitInternal(ref WebPPicture wpic)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureInitInternal_x86(ref wpic, WEBP_DECODER_ABI_VERSION);

                case 8:
                    return WebPPictureInitInternal_x64(ref wpic, WEBP_DECODER_ABI_VERSION);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPPictureInitInternal")]
        private static extern int WebPPictureInitInternal_x86(ref WebPPicture wpic, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPPictureInitInternal")]
        private static extern int WebPPictureInitInternal_x64(ref WebPPicture wpic, int WEBP_DECODER_ABI_VERSION);

        public static int WebPPictureImportBGR(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureImportBGR_x86(ref wpic, bgr, stride);

                case 8:
                    return WebPPictureImportBGR_x64(ref wpic, bgr, stride);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGR")]
        private static extern int WebPPictureImportBGR_x86(ref WebPPicture wpic, IntPtr bgr, int stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGR")]
        private static extern int WebPPictureImportBGR_x64(ref WebPPicture wpic, IntPtr bgr, int stride);

        public static int WebPPictureImportBGRX(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureImportBGRX_x86(ref wpic, bgr, stride);

                case 8:
                    return WebPPictureImportBGRX_x64(ref wpic, bgr, stride);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPPictureImportBGRX")]
        private static extern int WebPPictureImportBGRX_x86(ref WebPPicture wpic, IntPtr bgr, int stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPPictureImportBGRX")]
        private static extern int WebPPictureImportBGRX_x64(ref WebPPicture wpic, IntPtr bgr, int stride);

        public static int WebPEncode(ref WebPConfig config, ref WebPPicture picture)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncode_x86(ref config, ref picture);

                case 8:
                    return WebPEncode_x64(ref config, ref picture);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncode")]
        private static extern int WebPEncode_x86(ref WebPConfig config, ref WebPPicture picture);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncode")]
        private static extern int WebPEncode_x64(ref WebPConfig config, ref WebPPicture picture);

        public static void WebPPictureFree(ref WebPPicture picture)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPPictureFree_x86(ref picture);
                    break;

                case 8:
                    WebPPictureFree_x64(ref picture);
                    break;

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureFree")]
        private static extern void WebPPictureFree_x86(ref WebPPicture wpic);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureFree")]
        private static extern void WebPPictureFree_x64(ref WebPPicture wpic);

        public static int WebPGetInfo(IntPtr data, int data_size, out int width, out int height)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPGetInfo_x86(data, (UIntPtr)data_size, out width, out height);

                case 8:
                    return WebPGetInfo_x64(data, (UIntPtr)data_size, out width, out height);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetInfo")]
        private static extern int WebPGetInfo_x86([In] IntPtr data, UIntPtr data_size, out int width, out int height);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetInfo")]
        private static extern int WebPGetInfo_x64([In] IntPtr data, UIntPtr data_size, out int width, out int height);

        public static int WebPDecodeBGRInto(IntPtr data, int data_size, IntPtr output_buffer, int output_buffer_size,
            int output_stride)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPDecodeBGRInto_x86(data, (UIntPtr)data_size, output_buffer, output_buffer_size,
                        output_stride);

                case 8:
                    return WebPDecodeBGRInto_x64(data, (UIntPtr)data_size, output_buffer, output_buffer_size,
                        output_stride);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
        private static extern int WebPDecodeBGRInto_x86([In] IntPtr data, UIntPtr data_size, IntPtr output_buffer,
            int output_buffer_size, int output_stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
        private static extern int WebPDecodeBGRInto_x64([In] IntPtr data, UIntPtr data_size, IntPtr output_buffer,
            int output_buffer_size, int output_stride);

        public static int WebPInitDecoderConfig(ref WebPDecoderConfig webPDecoderConfig)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPInitDecoderConfigInternal_x86(ref webPDecoderConfig, WEBP_DECODER_ABI_VERSION);

                case 8:
                    return WebPInitDecoderConfigInternal_x64(ref webPDecoderConfig, WEBP_DECODER_ABI_VERSION);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPInitDecoderConfigInternal")]
        private static extern int WebPInitDecoderConfigInternal_x86(ref WebPDecoderConfig webPDecoderConfig,
            int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPInitDecoderConfigInternal")]
        private static extern int WebPInitDecoderConfigInternal_x64(ref WebPDecoderConfig webPDecoderConfig,
            int WEBP_DECODER_ABI_VERSION);

        public static VP8StatusCode WebPDecode(IntPtr data, int data_size, ref WebPDecoderConfig webPDecoderConfig)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPDecode_x86(data, (UIntPtr)data_size, ref webPDecoderConfig);

                case 8:
                    return WebPDecode_x64(data, (UIntPtr)data_size, ref webPDecoderConfig);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecode")]
        private static extern VP8StatusCode
            WebPDecode_x86(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecode")]
        private static extern VP8StatusCode
            WebPDecode_x64(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

        public static void WebPFreeDecBuffer(ref WebPDecBuffer buffer)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPFreeDecBuffer_x86(ref buffer);
                    break;

                case 8:
                    WebPFreeDecBuffer_x64(ref buffer);
                    break;

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFreeDecBuffer")]
        private static extern void WebPFreeDecBuffer_x86(ref WebPDecBuffer buffer);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFreeDecBuffer")]
        private static extern void WebPFreeDecBuffer_x64(ref WebPDecBuffer buffer);

        public static int WebPEncodeBGR(IntPtr bgr, int width, int height, int stride, float quality_factor,
            out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeBGR_x86(bgr, width, height, stride, quality_factor, out output);

                case 8:
                    return WebPEncodeBGR_x64(bgr, width, height, stride, quality_factor, out output);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGR")]
        private static extern int WebPEncodeBGR_x86([In] IntPtr bgr, int width, int height, int stride,
            float quality_factor, out IntPtr output);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGR")]
        private static extern int WebPEncodeBGR_x64([In] IntPtr bgr, int width, int height, int stride,
            float quality_factor, out IntPtr output);

        public static int WebPEncodeLosslessBGR(IntPtr bgr, int width, int height, int stride, out IntPtr output)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPEncodeLosslessBGR_x86(bgr, width, height, stride, out output);

                case 8:
                    return WebPEncodeLosslessBGR_x64(bgr, width, height, stride, out output);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPEncodeLosslessBGR")]
        private static extern int WebPEncodeLosslessBGR_x86([In] IntPtr bgr, int width, int height, int stride,
            out IntPtr output);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPEncodeLosslessBGR")]
        private static extern int WebPEncodeLosslessBGR_x64([In] IntPtr bgr, int width, int height, int stride,
            out IntPtr output);

        public static void WebPFree(IntPtr p)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPFree_x86(p);
                    break;

                case 8:
                    WebPFree_x64(p);
                    break;

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFree")]
        private static extern void WebPFree_x86(IntPtr p);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFree")]
        private static extern void WebPFree_x64(IntPtr p);

        public static int WebPGetDecoderVersion()
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPGetDecoderVersion_x86();

                case 8:
                    return WebPGetDecoderVersion_x64();

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPGetDecoderVersion")]
        private static extern int WebPGetDecoderVersion_x86();

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPGetDecoderVersion")]
        private static extern int WebPGetDecoderVersion_x64();

        public static int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type,
            IntPtr pResult)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return WebPPictureDistortion_x86(ref srcPicture, ref refPicture, metric_type, pResult);

                case 8:
                    return WebPPictureDistortion_x64(ref srcPicture, ref refPicture, metric_type, pResult);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPPictureDistortion")]
        private static extern int WebPPictureDistortion_x86(ref WebPPicture srcPicture, ref WebPPicture refPicture,
            int metric_type, IntPtr pResult);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "WebPPictureDistortion")]
        private static extern int WebPPictureDistortion_x64(ref WebPPicture srcPicture, ref WebPPicture refPicture,
            int metric_type, IntPtr pResult);
    }

    #endregion | Import libwebp functions |

    #region | Predefined |

    public enum WebPPreset
    {
        WEBP_PRESET_DEFAULT = 0,

        WEBP_PRESET_PICTURE,

        WEBP_PRESET_PHOTO,

        WEBP_PRESET_DRAWING,

        WEBP_PRESET_ICON,

        WEBP_PRESET_TEXT
    }

    public enum WebPEncodingError
    {
        VP8_ENC_OK = 0,

        VP8_ENC_ERROR_OUT_OF_MEMORY,

        VP8_ENC_ERROR_BITSTREAM_OUT_OF_MEMORY,

        VP8_ENC_ERROR_NULL_PARAMETER,

        VP8_ENC_ERROR_INVALID_CONFIGURATION,

        VP8_ENC_ERROR_BAD_DIMENSION,

        VP8_ENC_ERROR_PARTITION0_OVERFLOW,

        VP8_ENC_ERROR_PARTITION_OVERFLOW,

        VP8_ENC_ERROR_BAD_WRITE,

        VP8_ENC_ERROR_FILE_TOO_BIG,

        VP8_ENC_ERROR_USER_ABORT,

        VP8_ENC_ERROR_LAST
    }

    public enum VP8StatusCode
    {
        VP8_STATUS_OK = 0,

        VP8_STATUS_OUT_OF_MEMORY,

        VP8_STATUS_INVALID_PARAM,
        VP8_STATUS_BITSTREAM_ERROR,

        VP8_STATUS_UNSUPPORTED_FEATURE,

        VP8_STATUS_SUSPENDED,

        VP8_STATUS_USER_ABORT,

        VP8_STATUS_NOT_ENOUGH_DATA
    }

    public enum WebPImageHint
    {
        WEBP_HINT_DEFAULT = 0,

        WEBP_HINT_PICTURE,

        WEBP_HINT_PHOTO,

        WEBP_HINT_GRAPH,

        WEBP_HINT_LAST
    }

    public enum WEBP_CSP_MODE
    {
        MODE_RGB = 0,

        MODE_RGBA = 1,

        MODE_BGR = 2,

        MODE_BGRA = 3,

        MODE_ARGB = 4,

        MODE_RGBA_4444 = 5,

        MODE_RGB_565 = 6,

        MODE_rgbA = 7,

        MODE_bgrA = 8,

        MODE_Argb = 9,

        MODE_rgbA_4444 = 10,

        MODE_YUV = 11,

        MODE_YUVA = 12,

        MODE_LAST = 13
    }

    #endregion | Predefined |

    #region | libwebp structs |

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPBitstreamFeatures
    {
        public int width;

        public int height;

        public int has_alpha;

        public int has_animation;

        public int format;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPConfig
    {
        public int lossless;

        public float quality;

        public int method;

        public WebPImageHint image_hint;

        public int target_size;

        public float target_PSNR;

        public int segments;

        public int sns_strength;

        public int filter_strength;

        public int filter_sharpness;

        public int filter_type;

        public int autofilter;

        public int alpha_compression;

        public int alpha_filtering;

        public int alpha_quality;

        public int pass;

        public int show_compressed;

        public int preprocessing;

        public int partitions;

        public int partition_limit;

        public int emulate_jpeg_size;

        public int thread_level;

        public int low_memory;

        public int near_lossless;

        public int exact;

        public int delta_palettization;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPPicture
    {
        public int use_argb;

        public uint colorspace;

        public int width;

        public int height;

        public IntPtr y;

        public IntPtr u;

        public IntPtr v;

        public int y_stride;

        public int uv_stride;

        public IntPtr a;

        public int a_stride;

        public IntPtr argb;

        public int argb_stride;

        public IntPtr writer;

        public IntPtr custom_ptr;

        public int extra_info_type;

        public IntPtr extra_info;

        public IntPtr stats;

        public uint error_code;

        public IntPtr progress_hook;

        public IntPtr user_data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPAuxStats
    {
        public int coded_size;

        public float PSNRY;

        public float PSNRU;

        public float PSNRV;

        public float PSNRALL;

        public float PSNRAlpha;

        public int block_count_intra4;

        public int block_count_intra16;

        public int block_count_skipped;

        public int header_bytes;

        public int mode_partition_0;

        public int residual_bytes_DC_segments0;

        public int residual_bytes_AC_segments0;

        public int residual_bytes_uv_segments0;

        public int residual_bytes_DC_segments1;

        public int residual_bytes_AC_segments1;

        public int residual_bytes_uv_segments1;

        public int residual_bytes_DC_segments2;

        public int residual_bytes_AC_segments2;

        public int residual_bytes_uv_segments2;

        public int residual_bytes_DC_segments3;

        public int residual_bytes_AC_segments3;

        public int residual_bytes_uv_segments3;

        public int segment_size_segments0;

        public int segment_size_segments1;

        public int segment_size_segments2;

        public int segment_size_segments3;

        public int segment_quant_segments0;

        public int segment_quant_segments1;

        public int segment_quant_segments2;

        public int segment_quant_segments3;

        public int segment_level_segments0;

        public int segment_level_segments1;

        public int segment_level_segments2;

        public int segment_level_segments3;

        public int alpha_data_size;

        public int layer_data_size;

        public int lossless_features;

        public int histogram_bits;

        public int transform_bits;

        public int cache_bits;

        public int palette_size;

        public int lossless_size;

        public int lossless_hdr_size;

        public int lossless_data_size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecoderConfig
    {
        public WebPBitstreamFeatures input;

        public WebPDecBuffer output;

        public WebPDecoderOptions options;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecBuffer
    {
        public WEBP_CSP_MODE colorspace;

        public int width;

        public int height;

        public int is_external_memory;

        public RGBA_YUVA_Buffer u;

        public IntPtr private_memory;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RGBA_YUVA_Buffer
    {
        [FieldOffset(0)] public WebPRGBABuffer RGBA;

        [FieldOffset(0)] public WebPYUVABuffer YUVA;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPYUVABuffer
    {
        public IntPtr y;

        public IntPtr u;

        public IntPtr v;

        public IntPtr a;

        public int y_stride;

        public int u_stride;

        public int v_stride;

        public int a_stride;

        public UIntPtr y_size;

        public UIntPtr u_size;

        public UIntPtr v_size;

        public UIntPtr a_size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPRGBABuffer
    {
        public IntPtr rgba;

        public int stride;

        public UIntPtr size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecoderOptions
    {
        public int bypass_filtering;

        public int no_fancy_upsampling;

        public int use_cropping;

        public int crop_left;

        public int crop_top;

        public int crop_width;

        public int crop_height;

        public int use_scaling;

        public int scaled_width;

        public int scaled_height;

        public int use_threads;

        public int dithering_strength;

        public int flip;

        public int alpha_dithering_strength;
    }

    #endregion | libwebp structs |
}