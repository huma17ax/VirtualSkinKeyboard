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

sh_image1 = SharedData()
sh_image2 = SharedData()
sh_landmarks1 = SharedData()
sh_landmarks2 = SharedData()
sh_image_and_landmarks = SharedData()
sh_touches = SharedData()

image_sender = ImageSender(sh_image1, sh_landmarks2)
landmarks_sender = LandmarksSender(sh_landmarks1)
touches_sender = TouchesSender(sh_touches)
image_sender.start()
landmarks_sender.start()
touches_sender.start()

tracker = HandTracker(sh_image2, sh_landmarks1, sh_landmarks2, sh_image_and_landmarks)
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
    raise