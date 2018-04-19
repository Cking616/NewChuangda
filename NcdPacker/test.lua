ir_robot_state:SendCmd("restore ncd")
ir_robot_state:MoveStation("HOME", true, false, false, 1, 30)

while ir_robot_state.IsIdle == false do
	TaskManager.NcdSleep(100)
end

ir_robot_state:MoveStation("PA", true, false, false, 1, 30)
while ir_robot_state.IsIdle == false do
	TaskManager.NcdSleep(100)
end

ir_robot_state:MoveStation("BUFFER", true, false, false, 1, 30)

while ir_robot_state.IsIdle == false do
	TaskManager.NcdSleep(100)
end

io_controller_state:WriteOutput(1, 0, true)
TaskManager.NcdSleep(1000)
io_controller_state:WriteOutput(1, 0, false)

z_step_state: