class_name ConcurrentExample extends Node

const STOP_SIGNAL = -1
const NUMBER = 100

var queue : ConcurrentQueue = ConcurrentQueue.new()

func push_stop():
	queue.push(STOP_SIGNAL)

func create_producer():
	var producer = Thread.new()
	producer.start(self.producer_func)
	
	return producer

func producer_func():
	for i in range(NUMBER):
		queue.push(1)
		
		await get_tree().create_timer(0.0075).timeout
	
	print("Producer finished")

func create_consumer():
	var consumer = Thread.new()
	consumer.start(self.consumer_func)
	
	return consumer

func consumer_func():
	var count = 0
	var lastData = 0
	
	while lastData != STOP_SIGNAL:
		lastData = await queue.pop()
		count += lastData
		
		await get_tree().create_timer(0.005).timeout
	
	print("Consumer finished: %d" % count)
