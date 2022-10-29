from threading import Thread
from logger import logging
import mediapipe as mp

import time

mp_hands = mp.solutions.hands
POS = mp.solutions.hands.HandLandmark

class HandTracker(Thread):
    def __init__(self, sh_image, sh_landmarks1, sh_landmarks2, sh_image_and_landmarks):
        super(HandTracker, self).__init__()
        self.stop_flg = False
        self.sh_image = sh_image
        self.sh_landmarks1 = sh_landmarks1
        self.sh_landmarks2 = sh_landmarks2
        self.sh_image_and_landmarks = sh_image_and_landmarks
    
    def run(self):
        # In thread
        print('HAND TRACKER START.')
        with mp_hands.Hands(
            model_complexity=0,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5) as hands:
            
            while not self.stop_flg:
                image = self.sh_image.try_get()
                if image is None:
                    time.sleep(0.02)
                else:
                    results = hands.process(image)

                    if not results.multi_hand_landmarks:
                        logging([])
                        self.sh_landmarks1.set([])
                        self.sh_landmarks2.set([])
                        continue

                    hand_landmarks = results.multi_hand_landmarks[0]

                    self.sh_image_and_landmarks.set((image, hand_landmarks.landmark))
                    fingertips = [
                        (hand_landmarks.landmark[POS.INDEX_FINGER_TIP].x, hand_landmarks.landmark[POS.INDEX_FINGER_TIP].y),
                        (hand_landmarks.landmark[POS.MIDDLE_FINGER_TIP].x, hand_landmarks.landmark[POS.MIDDLE_FINGER_TIP].y),
                        (hand_landmarks.landmark[POS.RING_FINGER_TIP].x, hand_landmarks.landmark[POS.RING_FINGER_TIP].y),
                        (hand_landmarks.landmark[POS.PINKY_TIP].x, hand_landmarks.landmark[POS.PINKY_TIP].y)
                    ]
                    logging(fingertips)
                    self.sh_landmarks1.set(fingertips)
                    self.sh_landmarks2.set([hand_landmarks])
        
        print('HAND TRACKER END.')
    
    def stop(self):
        self.stop_flg = True