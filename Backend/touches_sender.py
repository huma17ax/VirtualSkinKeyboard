from concurrent.futures import thread
from namedpipe import NamedPipeClient
from threading import Thread
import time
import struct

class TouchesSender(Thread):
    def __init__(self, sh_touches):
        super(TouchesSender, self).__init__()
        self.stop_flg = False
        self.sh_touches = sh_touches
        self.pipe = NamedPipeClient('TouchesPipe')
        self.pipe.connect()
    
    def run(self):
        # In thread
        print('TOUCHES SENDER SATRT.')
        while not self.stop_flg:
            touches = self.sh_touches.try_get()
            if touches is not None:
                byte_touches = struct.pack('<????', *touches)
                self.pipe.write(byte_touches)
            time.sleep(0.02)
        print('TOUCHES SENDER END.')
    
    def stop(self):
        self.stop_flg = True