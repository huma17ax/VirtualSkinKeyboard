import cv2
import time

from marker_detector import detect_marker

from hand_tracker import HandTracker
from image_sender import ImageSender
from landmarks_sender import LandmarksSender

from shareddata import SharedData
from fisheye_undistort import undistort
from touch_detector import TouchDetector
from touches_sender import TouchesSender

CAMERA_INDEX = 0

capture = cv2.VideoCapture(CAMERA_INDEX)

print(capture.get(cv2.CAP_PROP_FPS))

sh_image1 = SharedData("image1")
sh_image2 = SharedData("image2")
sh_landmarks = SharedData("landmarks")
sh_image_and_landmarks = SharedData("image_and_land")
sh_touches = SharedData("touches")

image_sender = ImageSender(sh_image1)
landmarks_sender = LandmarksSender(sh_landmarks)
touches_sender = TouchesSender(sh_touches)
image_sender.start()
landmarks_sender.start()
touches_sender.start()

tracker = HandTracker(sh_image2, sh_landmarks, sh_image_and_landmarks)
detector = TouchDetector(sh_image_and_landmarks, sh_touches)
tracker.start()
detector.start()

try:
    while True:
        ret, frame = capture.read()
        frame = cv2.flip(frame, -1)
        frame = undistort(frame)

        sh_image1.set(detect_marker(frame))
        sh_image2.set(frame)

        time.sleep(0.02)
except:
    image_sender.stop()
    landmarks_sender.stop()
    touches_sender.stop()
    tracker.stop()
    detector.stop()
    print("sh_image1: ", end="")
    sh_image1.show_count()
    print("sh_image2: ", end="")
    sh_image2.show_count()
    print("sh_landmarks: ", end="")
    sh_landmarks.show_count()
    print("sh_image_and_landmarks: ", end="")
    sh_image_and_landmarks.show_count()
    print("sh_touches: ", end="")
    sh_touches.show_count()

    import json
    with open('.\hist.json', 'w') as f:
        json.dump(SharedData.histories, f)

    raise