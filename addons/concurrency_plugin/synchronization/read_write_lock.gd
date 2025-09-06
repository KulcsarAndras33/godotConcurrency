class_name ReadWriteLock extends Object

var _readers : AtomicInteger = AtomicInteger.new()
var _write_lock : Mutex = Mutex.new()
var _no_readers : Semaphore = Semaphore.new()

func readLock():
	_write_lock.lock()
	_readers.change(+1)
	_write_lock.unlock()

func readUnlock():
	_readers.change(-1)

func writeLock():
	_write_lock.lock()
	
	while _readers.val() > 0:
		_no_readers.wait()

func writeUnlock():
	_write_lock.unlock()
