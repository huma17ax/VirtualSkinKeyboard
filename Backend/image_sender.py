from namedpipe import NamedPipeClient
from threading import Thread
import time

class ImageSender(Thread):
    def __init__(self, sh_image):
        super(ImageSender, self).__init__()
        self.stop_flg = False
        self.sh_image = sh_image
        self.pipe = NamedPipeClient('ImagePipe')
        self.pipe.connect()
    
    def run(self):
        # In thread
        print('IMAGE SENDER START.')
        while not self.stop_flg:
            image = self.sh_image.try_get()
            if image is not None:
                byte_image = image[:,:,[2,1,0]].tobytes()
                self.pipe.write(byte_image)
            time.sleep(0.02)
        print('IMAGE SENDER END.')
    
    def stop(self):
        self.stop_flg = True