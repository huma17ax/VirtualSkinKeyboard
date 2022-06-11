using OpenCvSharp;
using UnityEngine;

public class OpenCV
{

    public static Texture2D GrayScale(WebCamTexture texture) {
        Mat src = OpenCvSharp.Unity.TextureToMat(texture);

        Mat dst = new Mat();

        Cv2.CvtColor(src, dst, ColorConversionCodes.RGB2GRAY);

        return OpenCvSharp.Unity.MatToTexture(dst);
    }

    public static Texture2D Flipped(WebCamTexture texture) {
        Mat src = OpenCvSharp.Unity.TextureToMat(texture);

        Mat dst = new Mat();
        
        Cv2.Flip(src, dst, FlipMode.XY);

        return OpenCvSharp.Unity.MatToTexture(dst);
    }

    public static Texture2D Undistorted(Texture2D texture) {
        Mat src = OpenCvSharp.Unity.TextureToMat(texture);

        Mat dst = new Mat();

        Cv2.Undistort(src, dst,
            InputArray.Create(CameraParams.cameraMatrix),
            InputArray.Create(CameraParams.distCoeffs),
            InputArray.Create(CameraParams.optimalNewCameraMatrix));

        return OpenCvSharp.Unity.MatToTexture(dst);
    }

    public static Texture2D Cropped(Texture2D texture) {
        Mat src = OpenCvSharp.Unity.TextureToMat(texture);

        OpenCvSharp.Rect roi = new OpenCvSharp.Rect(
            CameraParams.regionOfInterest[0],
            CameraParams.regionOfInterest[1],
            CameraParams.regionOfInterest[2],
            CameraParams.regionOfInterest[3]
        );

        // Mat dst = new Mat(
        //     CameraParams.regionOfInterest[3],
        //     CameraParams.regionOfInterest[2],
        //     MatType.CV_8UC3);
        
        // new Mat(src, roi).CopyTo(dst);

        Mat dst = new Mat(src, roi);

        return OpenCvSharp.Unity.MatToTexture(dst);
    }

}