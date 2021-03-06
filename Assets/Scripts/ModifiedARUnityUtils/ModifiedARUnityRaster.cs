/*
Texture2Dを使えるように修正
from NyARToolKit 5.0.8 NyARUnityRaster.cs
*/

using System;
using System.Diagnostics;
using UnityEngine;
using jp.nyatla.nyartoolkit.cs.core;
namespace ModifiedARUnityUtils
{

    /// <summary>
    /// Unity
    /// 
    /// </summary>
    /// <exception cref='NyARException'>
    /// Is thrown when the ny AR exception.
    /// </exception>
    public class ModifiedARUnityRaster : NyARRgbRaster
    {
        private Color32[] _buf;
        readonly private bool _is_inverse;
        public bool isFlipVirtical()
        {
            return this._is_inverse;
        }
        /// <summary>
        /// この関数は、Texture2Dを参照するインスタンスを生成します。
        /// </summary>
        /// <param name='i_tex'>
        /// I_tex.
        /// </param>/
        public ModifiedARUnityRaster(Texture2D i_tex, bool i_is_inverse)
            : base(i_tex.width, i_tex.height, false)
        {
            this._buf = i_tex.GetPixels32();
            this._is_inverse = i_is_inverse;
        }
        public ModifiedARUnityRaster(Texture2D i_tex)
            : this(i_tex, false)
        {

        }
        /**
         * この関数は、ラスタに外部参照バッファをセットします。
         * 外部参照バッファの時にだけ使えます。
         */
        public override void wrapBuffer(object i_ref_buf)
        {
            throw new NyARRuntimeException();
        }
        /**
		 * WebTextureで更新します。
		 */
        public void updateByTexture(Texture2D i_tex)
        {
            this._buf = i_tex.GetPixels32();
            //this._rgb_pixel_driver.switchRaster(this);//バッファを上書きするからいらない。
            return;
        }
        public override object createInterface(Type iIid)
        {
            if (iIid == typeof(INyARPerspectiveCopy))
            {
                return new PerspectiveCopy_Unity(this, this._is_inverse);
            }
            if (iIid == typeof(NyARMatchPattDeviationColorData.IRasterDriver))
            {
                //should be implement!
            }
            if (iIid == typeof(INyARRgb2GsFilter))
            {
                return new NyARRgb2GsFilterRgbAve_UnityRaster(this, this._is_inverse);
            }
            else if (iIid == typeof(INyARRgb2GsFilterRgbAve))
            {
                return new NyARRgb2GsFilterRgbAve_UnityRaster(this, this._is_inverse);
            }
            else if (iIid == typeof(INyARRgb2GsFilterArtkTh))
            {
                //may be implement?
            }
            return base.createInterface(iIid);
        }
        override public int getBufferType()
        {
            return NyARBufferType.OBJECT_CS_Unity;
        }

        /**
         * この関数は、指定した座標の1ピクセル分のRGBデータを、配列に格納して返します。
         */
        override public int[] getPixel(int i_x, int i_y, int[] o_rgb)
        {
            Color32 pix;
            if (this._is_inverse)
            {
                //byte(BGRX)=int(XRGB)
                pix = this._buf[i_x + (this._size.h - 1 - i_y) * this._size.w];
            }
            else
            {
                pix = this._buf[i_x + i_y * this._size.w];
            }
            o_rgb[0] = pix.r;// R
            o_rgb[1] = pix.g; // G
            o_rgb[2] = pix.b;    // B
            return o_rgb;
        }

        /**
         * この関数は、座標群から、ピクセルごとのRGBデータを、配列に格納して返します。
         */
        override public int[] getPixelSet(int[] i_x, int[] i_y, int i_num, int[] o_rgb)
        {
            if (this._is_inverse)
            {
                int h1 = this._size.h - 1;
                for (int i = i_num - 1; i >= 0; i--)
                {
                    Color32 pix = this._buf[i_x[i] + (h1 - i_y[i]) * this._size.w];
                    o_rgb[i * 3 + 0] = pix.r;// R
                    o_rgb[i * 3 + 1] = pix.g; // G
                    o_rgb[i * 3 + 2] = pix.b;    // B
                }
            }
            else
            {
                for (int i = i_num - 1; i >= 0; i--)
                {
                    Color32 pix = this._buf[i_x[i] + i_y[i] * this._size.w];
                    o_rgb[i * 3 + 0] = pix.r;// R
                    o_rgb[i * 3 + 1] = pix.g; // G
                    o_rgb[i * 3 + 2] = pix.b;    // B
                }
            }
            return o_rgb;
        }

        /**
         * この関数は、RGBデータを指定した座標のピクセルにセットします。
         */
        override public void setPixel(int i_x, int i_y, int[] i_rgb)
        {
            int idx;
            if (this._is_inverse)
            {
                idx = i_x + (this._size.h - 1 - i_y) * this._size.w;
            }
            else
            {
                idx = i_x + i_y * this._size.w;
            }
            this._buf[idx].r = (byte)i_rgb[0];
            this._buf[idx].g = (byte)i_rgb[1];
            this._buf[idx].b = (byte)i_rgb[2];
            return;
        }

        /**
         * この関数は、RGBデータを指定した座標のピクセルにセットします。
         */
        override public void setPixel(int i_x, int i_y, int i_r, int i_g, int i_b)
        {
            int idx;
            if (this._is_inverse)
            {
                idx = i_x + (this._size.h - 1 - i_y) * this._size.w;
            }
            else
            {
                idx = i_x + i_y * this._size.w;
            }
            this._buf[idx].r = (byte)i_r;
            this._buf[idx].g = (byte)i_g;
            this._buf[idx].b = (byte)i_b;
            return;
        }

        /**
         * この関数は、機能しません。
         */
        override public void setPixels(int[] i_x, int[] i_y, int i_num, int[] i_intrgb)
        {
            NyARRuntimeException.notImplement();
        }
        override public System.Object getBuffer()
        {
            return this._buf;
        }

    }

    #region pixel drivers
    sealed class NyARRgb2GsFilterRgbAve_UnityRaster : INyARRgb2GsFilterRgbAve
    {
		private bool _is_inverse;
        private ModifiedARUnityRaster _ref_raster;
        public NyARRgb2GsFilterRgbAve_UnityRaster(ModifiedARUnityRaster i_ref_raster,bool is_inverse)
        {
            System.Diagnostics.Debug.Assert(i_ref_raster.isEqualBufferType(NyARBufferType.OBJECT_CS_Unity));
            this._ref_raster = i_ref_raster;
			this._is_inverse = is_inverse;
        }
        public void convert(INyARGrayscaleRaster i_raster)
        {
            NyARIntSize s = this._ref_raster.getSize();
            this.convertRect(0, 0, s.w, s.h, i_raster);
        }
        public void convertRect(int l, int t, int w, int h, INyARGrayscaleRaster o_raster)
        {
			Color32[] c=(Color32[])(this._ref_raster.getBuffer());
            NyARIntSize size = this._ref_raster.getSize();
            int src = (l + t * size.w) * 4;
            int b = t + h;
            int row_padding_dst = (size.w - w);
            int row_padding_src = row_padding_dst * 4;
            int pix_count = w;
            int pix_mod_part = pix_count - (pix_count % 8);
            
            // in_buf = (byte[])this._ref_raster.getBuffer();
            switch (o_raster.getBufferType())
            {
                case NyARBufferType.INT1D_GRAY_8:
                    int[] out_buf = (int[])o_raster.getBuffer();
					int dst_ptr;
					if(this._is_inverse){
						dst_ptr=(h-1-t) * size.w + l;
						row_padding_dst-=size.w*2;
					}else{
						dst_ptr=t * size.w + l;
					}
                    for (int y = t; y < b; y++)
                    {						
                        int x = 0;
						Color32 p;
                        for (x = pix_count - 1; x >= pix_mod_part; x--)
                        {
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
                        }
                        for (; x >= 0; x -= 8)
                        {
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
							p=c[src++];
                            out_buf[dst_ptr++] = (p.r+p.g+p.b) / 3;
                        }
						dst_ptr+=row_padding_dst;
                        src += row_padding_src;
                    }
                    return;
                default:
                    for (int y = t; y < b; y++)
                    {
                        for (int x = 0; x < pix_count; x++)
                        {
                            Color32 p = c[src++];
                            o_raster.setPixel(x, y,(p.r+p.g+p.b) / 3);
                        }
                        src += row_padding_src;
                    }
                    return;
            }

        }
    }




    sealed class PerspectiveCopy_Unity : NyARPerspectiveCopy_Base
    {
        private ModifiedARUnityRaster _ref_raster;
        private bool _is_inv_v = false;
        public PerspectiveCopy_Unity(ModifiedARUnityRaster i_ref_raster, bool i_inv_v)
        {
            System.Diagnostics.Debug.Assert(i_ref_raster.isEqualBufferType(NyARBufferType.OBJECT_CS));
            this._is_inv_v = i_inv_v;
            this._ref_raster = i_ref_raster;
        }

        protected override bool onePixel(int pk_l, int pk_t, double[] cpara, INyARRaster o_out)
        {
            Color32[] in_pixs = (Color32[])this._ref_raster.getBuffer();
            int in_w = this._ref_raster.getWidth();
            int in_h = this._ref_raster.getHeight();

            //ピクセルリーダーを取得
            double cp0 = cpara[0];
            double cp3 = cpara[3];
            double cp6 = cpara[6];
            double cp1 = cpara[1];
            double cp4 = cpara[4];
            double cp7 = cpara[7];

            int out_w = o_out.getWidth();
            int out_h = o_out.getHeight();
            double cp7_cy_1 = cp7 * pk_t + 1.0 + cp6 * pk_l;
            double cp1_cy_cp2 = cp1 * pk_t + cpara[2] + cp0 * pk_l;
            double cp4_cy_cp5 = cp4 * pk_t + cpara[5] + cp3 * pk_l;
            int p;

            int step, offset;
            //flip Virtical
            switch (o_out.getBufferType())
            {
                case NyARBufferType.INT1D_X8R8G8B8_32:
                    int[] pat_data = (int[])o_out.getBuffer();
                    p = 0;
                    if (this._is_inv_v)
                    {
                        offset = in_w * (in_h - 1);
                        step = -in_w;
                    }
                    else
                    {
                        offset = 0;
                        step = in_w;
                    }
                    for (int iy = 0; iy < out_h; iy++)
                    {
                        //解像度分の点を取る。
                        double cp7_cy_1_cp6_cx = cp7_cy_1;
                        double cp1_cy_cp2_cp0_cx = cp1_cy_cp2;
                        double cp4_cy_cp5_cp3_cx = cp4_cy_cp5;

                        for (int ix = 0; ix < out_w; ix++)
                        {
                            //1ピクセルを作成
                            double d = 1 / (cp7_cy_1_cp6_cx);
                            int x = (int)((cp1_cy_cp2_cp0_cx) * d);
                            int y = (int)((cp4_cy_cp5_cp3_cx) * d);
                            if (x < 0) { x = 0; } else if (x >= in_w) { x = in_w - 1; }
                            if (y < 0) { y = 0; } else if (y >= in_h) { y = in_h - 1; }

                            Color32 pix = in_pixs[x + offset + step * y];
                            //
                            pat_data[p] = ((pix.r << 16) & 0xff) | ((pix.g << 8) & 0xff) | pix.b;
                            cp7_cy_1_cp6_cx += cp6;
                            cp1_cy_cp2_cp0_cx += cp0;
                            cp4_cy_cp5_cp3_cx += cp3;
                            p++;
                        }
                        cp7_cy_1 += cp7;
                        cp1_cy_cp2 += cp1;
                        cp4_cy_cp5 += cp4;
                    }
                    return true;
                case NyARBufferType.OBJECT_CS_Unity:
                    Color32[] out_buf = (Color32[])(((INyARRgbRaster)o_out).getBuffer());
                    if (this._is_inv_v == ((ModifiedARUnityRaster)o_out).isFlipVirtical())
                    {
                        offset = in_w * (in_h - 1);
                        step = -in_w;
                    }
                    else
                    {
                        offset = 0;
                        step = in_w;
                    }
                    for (int iy = 0; iy < out_h; iy++)
                    {
                        //解像度分の点を取る。
                        double cp7_cy_1_cp6_cx = cp7_cy_1;
                        double cp1_cy_cp2_cp0_cx = cp1_cy_cp2;
                        double cp4_cy_cp5_cp3_cx = cp4_cy_cp5;
                        int ys = out_h - 1 - iy;
                        for (int ix = 0; ix < out_w; ix++)
                        {
                            //1ピクセルを作成
                            double d = 1 / (cp7_cy_1_cp6_cx);
                            int x = (int)((cp1_cy_cp2_cp0_cx) * d);
                            int y = (int)((cp4_cy_cp5_cp3_cx) * d);
                            if (x < 0) { x = 0; } else if (x >= in_w) { x = in_w - 1; }
                            if (y < 0) { y = 0; } else if (y >= in_h) { y = in_h - 1; }

                            out_buf[ix + ys * out_w] = in_pixs[x + offset + step * y];
                            //
                            cp7_cy_1_cp6_cx += cp6;
                            cp1_cy_cp2_cp0_cx += cp0;
                            cp4_cy_cp5_cp3_cx += cp3;
                        }
                        cp7_cy_1 += cp7;
                        cp1_cy_cp2 += cp1;
                        cp4_cy_cp5 += cp4;
                    }
                    return true;
                default:
                    //ANY to RGBx
                    if (o_out is INyARRgbRaster)
                    {
                        INyARRgbRaster out_raster = ((INyARRgbRaster)o_out);
                        if (this._is_inv_v)
                        {
                            offset = in_w * (in_h - 1);
                            step = -in_w;
                        }
                        else
                        {
                            offset = 0;
                            step = in_w;
                        }
                        for (int iy = 0; iy < out_h; iy++)
                        {
                            //解像度分の点を取る。
                            double cp7_cy_1_cp6_cx = cp7_cy_1;
                            double cp1_cy_cp2_cp0_cx = cp1_cy_cp2;
                            double cp4_cy_cp5_cp3_cx = cp4_cy_cp5;

                            for (int ix = 0; ix < out_w; ix++)
                            {
                                //1ピクセルを作成
                                double d = 1 / (cp7_cy_1_cp6_cx);
                                int x = (int)((cp1_cy_cp2_cp0_cx) * d);
                                int y = (int)((cp4_cy_cp5_cp3_cx) * d);
                                if (x < 0) { x = 0; } else if (x >= in_w) { x = in_w - 1; }
                                if (y < 0) { y = 0; } else if (y >= in_h) { y = in_h - 1; }
                                Color32 px = in_pixs[x + offset + step * y];
                                cp7_cy_1_cp6_cx += cp6;
                                cp1_cy_cp2_cp0_cx += cp0;
                                cp4_cy_cp5_cp3_cx += cp3;
                                out_raster.setPixel(ix, iy, px.r, px.g, px.b);
                            }
                            cp7_cy_1 += cp7;
                            cp1_cy_cp2 += cp1;
                            cp4_cy_cp5 += cp4;
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }
        protected override bool multiPixel(int pk_l, int pk_t, double[] cpara, int i_resolution, INyARRaster o_out)
        {
            Color32[] in_pixs = (Color32[])this._ref_raster.getBuffer();
            int in_w = this._ref_raster.getWidth();
            int in_h = this._ref_raster.getHeight();
            int res_pix = i_resolution * i_resolution;

            //ピクセルリーダーを取得
            double cp0 = cpara[0];
            double cp3 = cpara[3];
            double cp6 = cpara[6];
            double cp1 = cpara[1];
            double cp4 = cpara[4];
            double cp7 = cpara[7];
            double cp2 = cpara[2];
            double cp5 = cpara[5];

            int step, offset;
            int out_w = o_out.getWidth();
            int out_h = o_out.getHeight();
            if (o_out is INyARRgbRaster)
            {
                INyARRgbRaster out_raster = ((INyARRgbRaster)o_out);
                if (this._is_inv_v)
                {
                    offset = in_w * (in_h - 1);
                    step = -in_w;
                }
                else
                {
                    offset = 0;
                    step = in_w;
                }
                for (int iy = out_h - 1; iy >= 0; iy--)
                {
                    //解像度分の点を取る。
                    for (int ix = out_w - 1; ix >= 0; ix--)
                    {
                        int r, g, b;
                        r = g = b = 0;
                        int cy = pk_t + iy * i_resolution;
                        int cx = pk_l + ix * i_resolution;
                        double cp7_cy_1_cp6_cx_b = cp7 * cy + 1.0 + cp6 * cx;
                        double cp1_cy_cp2_cp0_cx_b = cp1 * cy + cp2 + cp0 * cx;
                        double cp4_cy_cp5_cp3_cx_b = cp4 * cy + cp5 + cp3 * cx;
                        for (int i2y = i_resolution - 1; i2y >= 0; i2y--)
                        {
                            double cp7_cy_1_cp6_cx = cp7_cy_1_cp6_cx_b;
                            double cp1_cy_cp2_cp0_cx = cp1_cy_cp2_cp0_cx_b;
                            double cp4_cy_cp5_cp3_cx = cp4_cy_cp5_cp3_cx_b;
                            for (int i2x = i_resolution - 1; i2x >= 0; i2x--)
                            {
                                //1ピクセルを作成
                                double d = 1 / (cp7_cy_1_cp6_cx);
                                int x = (int)((cp1_cy_cp2_cp0_cx) * d);
                                int y = (int)((cp4_cy_cp5_cp3_cx) * d);
                                if (x < 0) { x = 0; } else if (x >= in_w) { x = in_w - 1; }
                                if (y < 0) { y = 0; } else if (y >= in_h) { y = in_h - 1; }
                                Color32 px = in_pixs[x + offset + step * y];
                                r += px.r;// R
                                g += px.g;// G
                                b += px.b;// B
                                cp7_cy_1_cp6_cx += cp6;
                                cp1_cy_cp2_cp0_cx += cp0;
                                cp4_cy_cp5_cp3_cx += cp3;
                            }
                            cp7_cy_1_cp6_cx_b += cp7;
                            cp1_cy_cp2_cp0_cx_b += cp1;
                            cp4_cy_cp5_cp3_cx_b += cp4;
                        }
                        out_raster.setPixel(ix, iy, r / res_pix, g / res_pix, b / res_pix);
                    }
                }
                return true;
            }
            return false;
        }
    }
    #endregion
}


