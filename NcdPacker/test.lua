function wait_for_flag( flag )
	while flag == false do
		TaskManager.NcdSleep(100)
	end
end

function sleep(mil_sec)
	TaskManager.NcdSleep(mil_sec)
end

ir_robot_state:SendCmd("restore ncd")
ir_robot_state:MoveStation("HOME", true, false, false, 1, 30)
y_step_state:SendCmd("#1y1")
wait_for_flag(y_step_state.IsIdle)
y_step_state:SendCmd("#1A")
wait_for_flag(y_step_state.IsIdle)

wait_for_flag(ir_robot_state.IsIdle)

z_step_state:SendCmd("#1y1")
wait_for_flag(z_step_state.IsIdle)
z_step_state:SendCmd("#1A")
wait_for_flag(z_step_state.IsIdle)

ir_robot_state:MoveStation("PA", true, false, false, 1, 30)

wait_for_flag(ir_robot_state.IsIdle)

ir_robot_state:MoveStation("BUFFER", true, false, false, 1, 30)

wait_for_flag(ir_robot_state.IsIdle)

io_controller_state:WriteOutput(1, 0, true)
sleep(1000)
io_controller_state:WriteOutput(1, 0, false)

ir_robot_state:MoveStation("PA", true, false, false, 1, 30)

z_step_state:SendCmd("#1y2")
wait_for_flag(z_step_state.IsIdle)
z_step_state:SendCmd("#1o=100")
wait_for_flag(z_step_state.IsIdle)
z_step_state:SendCmd("#1A")
wait_for_flag(z_step_state.IsIdle)

wait_for_flag(ir_robot_state.IsIdle)
sleep(2000)
