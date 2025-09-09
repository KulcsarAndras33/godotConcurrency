class_name ReadWriteLock extends Object

const DEBUG_LOG = false
const DEBUG_ONLY_WRITE = true

var _readers : AtomicInteger = AtomicInteger.new()
var _write_lock : Mutex = Mutex.new()
var _no_readers : Semaphore = Semaphore.new()

var name : String = "Unnamed"

func set_name(name : String) -> ReadWriteLock:
	self.name = name
	return self

func read_lock():
	_write_lock.lock()
	_readers.change(+1)
	_write_lock.unlock()
	
	debug_log("read lock")

func try_read_lock():
	if _write_lock.try_lock():
		_readers.change(+1)
		_write_lock.unlock()
		return true
	return false

func read_unlock():
	_readers.change(-1)
	if _readers.val() == 0:
		_no_readers.try_wait()
		_no_readers.post(1)
	
	debug_log("read unlock")

func write_lock():
	_write_lock.lock()
	
	while _readers.val() > 0:
		debug_log("write lock - waiting for readers")
		_no_readers.wait()
	debug_log("write lock")

func write_unlock():
	_write_lock.unlock()
	debug_log("write unlock")


func debug_log(msg : String):
	if !DEBUG_LOG:
		return
	
	if DEBUG_ONLY_WRITE and !msg.contains("write"):
		return
	
	print("%s: %s by %s" % [name, msg, OS.get_thread_caller_id()])
