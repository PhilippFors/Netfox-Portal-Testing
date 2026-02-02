class_name gd_NodeHelper extends Node

static func find_child_node(node: Node, type: Variant) -> Node:
	var children = node.get_children();
	for child in children:
		if is_instance_of(child, type):
			return child
	return
	
static func find_parent_node(node: Node, type: Variant) -> Node:
	var parent = node.get_parent()
	if is_instance_of(parent, type):
		return parent
	return

static func find_parent_node_cont(node: Node, type: Variant) -> Node:
	var parent = node.get_parent()
	if !parent:
		return
	if is_instance_of(parent, type):
		return parent
	return find_parent_node(parent, type)
