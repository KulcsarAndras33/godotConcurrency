class_name HierachicalPathfinder extends Object

const ABSTRACT_ERROR_MSG = "This is an abstract class, do not call methods directly!"

func add_point(pointData):
	push_error(ABSTRACT_ERROR_MSG)

func remove_point(pointData):
	push_error(ABSTRACT_ERROR_MSG)

func add_edge(edgeData):
	push_error(ABSTRACT_ERROR_MSG)

func remove_edge(edgeData):
	push_error(ABSTRACT_ERROR_MSG)

func get_path(from, to):
	push_error(ABSTRACT_ERROR_MSG)
