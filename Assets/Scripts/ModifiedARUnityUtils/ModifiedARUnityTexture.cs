/*
Texture2Dを使えるように修正
from NyARToolKit 5.0.8 NyARUnityWebCam.cs
*/

using UnityEngine;
using System.Collections;
using jp.nyatla.nyartoolkit.cs.markersystem;
using jp.nyatla.nyartoolkit.cs.core;

namespace ModifiedARUnityUtils
{
	/// <summary>
	/// This class provides WebCamTexture wrapper derived  from NyARMarkerSystemSensor.
	/// </summary>
	/// <exception cref='NyARException'>
	/// Is thrown when the ny AR exception.
	/// </exception>
	public class ModifiedARUnityTexture :NyARSensor
	{

		public int width{
			get{return this._raster.getWidth();}
		}
		public int height{
			get{return this._raster.getHeight();}
		}
		private Texture2D _wtx;
	    private ModifiedARUnityRaster _raster;
		/**
		 * WebcamTextureを元にインスタンスを生成します.
		 * 画像サイズを自分で設定できます.
		 * @param i_wtx
		 * Webカメラは開始されている必要があります.
		 * 
		 */
		public ModifiedARUnityTexture(Texture2D i_wtx): base(new NyARIntSize(i_wtx.width,i_wtx.height))
		{
	        //RGBラスタの生成(Webtextureは上下反転必要)
	        this._raster = new ModifiedARUnityRaster(new Texture2D(i_wtx.width,i_wtx.height,TextureFormat.RGBA32,false),true);
	        //ラスタのセット
	        base.update(this._raster);
			this._wtx=i_wtx;
		}
		/**
		 * Call this function on update!
		 */
		public void update()
		{
			//テクスチャがアップデートされていたら、ラスタを更新
			this._raster.updateByTexture(this._wtx);
			//センサのタイムスタンプを更新
			base.updateTimeStamp();
			return;
		}
        /// <summary>
        /// この関数は使わないでください。
        /// </summary>
        /// <param name="i_input"></param>
		public override void update(INyARRgbRaster i_input)
		{
			throw new NyARRuntimeException();
		}
		public void dGetGsTex(Texture2D tx)
		{
			int[] s=(int[])this._gs_raster.getBuffer();
			Color32[] c=new Color32[320*240];
			for(int i=0;i<240;i++){
				for(int i2=0;i2<320;i2++){
					c[i*320+i2].r=c[i*320+i2].g=c[i*320+i2].b=(byte)s[i*320+i2];
					c[i*320+i2].a=0xff;
				}
			}
			tx.SetPixels32(c);
			tx.Apply( false );
		}
	}

}

