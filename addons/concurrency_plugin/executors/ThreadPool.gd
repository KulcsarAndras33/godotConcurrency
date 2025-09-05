class_name ThreadPool extends Object

signal _task_finish

var _threads : Array[Thread] = []
var _task_lock : Semaphore = Semaphore.new()
var _task_queue : ConcurrentQueue = ConcurrentQueue.new()
var _shutdown : bool = false

func _init(threadCount : int) -> void:
	for i in range(threadCount):
		var thread = Thread.new()
		thread.start(self._thread_main)
		
		_threads.append(thread)

func _thread_main():
	while true:
		_task_lock.wait()
		if !_task_queue.peek():
			break
		
		var current_task : Callable = await _task_queue.pop()
		
		await current_task.call()
		
		_task_finish.emit()

func execute(task : Callable):
	if _shutdown:
		return false
	
	await _task_queue.push(task)
	_task_lock.post(1)
	
	return true

func shutdown():
	_shutdown = true
	
	while _task_queue.peek():
		await _task_finish
	
	_task_lock.post(_threads.size())
	
	for thread in _threads:
		await thread.wait_to_finish()
