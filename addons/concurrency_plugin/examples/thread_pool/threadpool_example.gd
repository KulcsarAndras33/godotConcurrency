extends Node

var example = ConcurrentExample.new()

func _ready() -> void:
	print_caller_id()
	add_child(example)
	
	var thread_pool = ThreadPool.new(5)
	
	thread_pool.execute(example.producer_func)
	thread_pool.execute(example.producer_func)
	
	thread_pool.execute(example.consumer_func)
	thread_pool.execute(example.consumer_func)
	
	await get_tree().create_timer(0.1).timeout
	example.push_stop()
	example.push_stop()
	
	print_caller_id()
	await thread_pool.shutdown()
	
	print("SUCCESSFUL SHUTDOWN")

func print_caller_id():
	print(OS.get_thread_caller_id())

func simple():
	await example.queue.push(1)
	
	print("SUCCESS")
	
