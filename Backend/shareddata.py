import queue

# 以下を満たすスレッドセーフな共有データ
# ・setでデータを更新できる
# ・getでデータを取り出せる
# ・一度getで取り出すと，再びsetされるまで取り出せなくなる

class SharedData():
    histories = []

    def __init__(self, name):
        self.name = name
        self.queue = queue.Queue(maxsize=1)
        self.count_set = 0
        self.count_set_failed = 0
        self.count_get = 0
        self.count_get_failed = 0

    def set(self, data):
        self.count_set += 1
        SharedData.histories.append((self.name, "set"))
        try:
            self.queue.get(block=False)
            self.count_set_failed += 1
            SharedData.histories.append((self.name, "set_failed"))
        except queue.Empty:
            pass
        self.queue.put(data)

    def get(self, timeout=None):
        self.queue.get(block=True, timeout=timeout)

    def try_get(self):
        self.count_get += 1
        SharedData.histories.append((self.name, "get"))
        try:
            return self.queue.get(block=False)
        except queue.Empty:
            self.count_get_failed += 1
            SharedData.histories.append((self.name, "get_failed"))
            return None

    def show_count(self):
        print((self.count_set, self.count_set_failed, self.count_get, self.count_get_failed))