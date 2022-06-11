from threading import Thread
from keras.models import load_model
import time

import preprocessing

import tensorflow as tf
physical_devices = tf.config.list_physical_devices('GPU')
if len(physical_devices) > 0:
    for device in physical_devices:
        tf.config.experimental.set_memory_growth(device, True)
        print('{} memory growth: {}'.format(device, tf.config.experimental.get_memory_growth(device)))
else:
    print("Not enough GPU hardware devices available")

class TouchDetector(Thread):
    def __init__(self, sh_image_and_landmarks, sh_touches):
        super(TouchDetector, self).__init__()
        self.stop_flg = False
        self.sh_image_and_landmarks = sh_image_and_landmarks
        self.sh_touches = sh_touches
        self.model = load_model('./model')
        self.model.compile()
    
    def run(self):
        # In thread
        print('TOUCH DETECTOR START')
        while not self.stop_flg:
            image_and_landmarks = self.sh_image_and_landmarks.try_get()
            if image_and_landmarks is None:
                time.sleep(0.02)
            else:
                image, landmarks = image_and_landmarks
                cropped_images = preprocessing.crop(image, landmarks)

                if cropped_images is None:
                    continue

                touches = self.model.predict([
                    image.reshape((1,1,50,50,3)) for image in cropped_images
                ])
                self.sh_touches.set([t[0][0][0] > 0.5 for t in touches])
        print('TOUCH DETECTOR END')
    
    def stop(self):
        self.stop_flg = True