extends Node

var example = ConcurrentExample.new()

func _ready() -> void:
	add_child(example)
	
	var producers : Array[Thread] = []
	producers.append(example.create_producer())
	producers.append(example.create_producer())
	producers.append(example.create_producer())
	
	var consumers : Array[Thread] = []
	
	consumers.append(example.create_consumer())
	consumers.append(example.create_consumer())
	
	for thread in producers:
		await thread.wait_to_finish()
	
	for thread in consumers:
		example.push_stop()
	
	for thread in consumers:
		await thread.wait_to_finish()
	
