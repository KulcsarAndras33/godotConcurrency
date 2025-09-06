class_name ReadWriteLock extends Object

var _readers : AtomicInteger = AtomicInteger.new()
var _write_lock : Mutex = Mutex.new()
var _no_readers : Semaphore = Semaphore.new()

func read_lock():
	_write_lock.lock()
	_readers.change(+1)
	_write_lock.unlock()

func try_read_lock():
	if _write_lock.try_lock():
		_readers.change(+1)
		_write_lock.unlock()
		return true
	return false

func read_unlock():
	_readers.change(-1)

func write_lock():
	_write_lock.lock()
	
	while _readers.val() > 0:
		_no_readers.wait()

func write_unlock():
	_write_lock.unlock()
