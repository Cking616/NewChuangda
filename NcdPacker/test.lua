function sleep(mil_sec)
	TaskManager.NcdSleep(mil_sec)
end

ir_robot_state:SendCmd("restore ncd")
ir_robot_state:MoveStation("HOME", true, false, false, 1, 30)

y_step_state:SendCmd("#1D")
y_step_state:SendCmd("#1y1")
y_step_state:SendCmd("#1A")

while ir_robot_state.CurStation ~= "HOME" do
	TaskManager.NcdSleep(100)
end

z_step_state:SendCmd("#1D")
z_step_state:SendCmd("#1y1")
z_step_state:SendCmd("#1A")

ir_robot_state:MoveStation("PA", true, false, false, 1, 30)

while ir_robot_state.CurStation ~= "PA" do
	TaskManager.NcdSleep(100)
end

ir_robot_state:MoveStation("BUFFER", true, false, false, 1, 30)

while ir_robot_state.CurStation ~= "BUFFER" do
	TaskManager.NcdSleep(100)
end

io_controller_state:WriteOutput(1, 0, true)
sleep(1000)
io_controller_state:WriteOutput(1, 0, false)

ir_robot_state:MoveStation("PA", true, false, false, 1, 30)

z_step_state:SendCmd("#1y6")
z_step_state:SendCmd("#1o=100")
z_step_state:SendCmd("#1A")


while ir_robot_state.CurStation ~= "PA" do
	TaskManager.NcdSleep(100)
end

sleep(2000)
