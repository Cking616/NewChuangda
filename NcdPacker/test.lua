require("ncdlib")
ir_robot_state:SendCmd("restore ncd")

while true do	
	ir_robot_state:MoveStation("HOME", true, false, false, 1, 50)

	z_home_cmd()

	while ir_robot_state.CurStation ~= "HOME" do
		TaskManager.NcdSleep(100)
	end

	ir_robot_state:MoveStation("PA", true, false, false, 1, 50)

	z_go_up_station_cmd()

	while ir_robot_state.CurStation ~= "PA" do
		TaskManager.NcdSleep(100)
	end

	ir_robot_state:MoveStation("BUFFER", true, false, false, 1, 50)

	y_home_cmd()

	while ir_robot_state.CurStation ~= "BUFFER" do
		TaskManager.NcdSleep(100)
	end

	io_controller_state:WriteOutput(1, 0, true)
	sleep(1000)
	io_controller_state:WriteOutput(1, 0, false)

	ir_robot_state:MoveStation("PA", true, false, false, 1, 30)

	while ir_robot_state.CurStation ~= "PA" do
		TaskManager.NcdSleep(100)
	end

	y_go_papar_station_cmd()

	sleep(10000)
end