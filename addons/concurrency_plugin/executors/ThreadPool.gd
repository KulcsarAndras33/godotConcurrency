class_name ThreadPool extends Object

signal _task_finish

var _threads : Array[Thread] = []
var _task_lock : Semaphore = Semaphore.new()
var _task_queue : ConcurrentPriorityQueue = ConcurrentPriorityQueue.new()
var _shutdown : bool = false

func _init(threadCount : int) -> void:
	for i in range(threadCount):
		var thread = Thread.new()
		_threads.append(thread)
		
		thread.start(self._thread_main)

func _thread_main():
	while true:
		_task_lock.wait()
		if !_task_queue.peek():
			break
		
		var current_task : Callable = await _task_queue.pop()
		
		print("BEFORE: %d" % OS.get_thread_caller_id())
		current_task.call()
		print("AFTER: %d" % OS.get_thread_caller_id())
		
		if OS.get_thread_caller_id() == 1:
			push_warning("""One of the threads in the thread pool is now running on the main thread.
			You might have left an await in your task.""")
		
		_task_finish.emit()

func execute(task : Callable, prio : int = 50):
	if _shutdown:
		return false
	
	await _task_queue.push(task, prio)
	_task_lock.post(1)
	
	return true

func shutdown():
	_shutdown = true
	
	while _task_queue.peek():
		await _task_finish
	
	_task_lock.post(_threads.size())
	
	for thread in _threads:
		if thread.is_alive():
			await thread.wait_to_finish()
