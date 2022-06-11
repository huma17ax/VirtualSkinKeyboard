import queue

# 以下を満たすスレッドセーフな共有データ
# ・setでデータを更新できる
# ・getでデータを取り出せる
# ・一度getで取り出すと，再びsetされるまで取り出せなくなる

class SharedData():
    def __init__(self):
        self.queue = queue.Queue(maxsize=1)

    def set(self, data):
        try:
            self.queue.get(block=False)
        except queue.Empty:
            pass
        self.queue.put(data)

    def get(self, timeout=None):
        self.queue.get(block=True, timeout=timeout)

    def try_get(self):
        try:
            return self.queue.get(block=False)
        except queue.Empty:
            return None
