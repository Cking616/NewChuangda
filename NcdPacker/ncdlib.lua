function sleep(mil_sec)
	TaskManager.NcdSleep(mil_sec)
end

function z_home_cmd()
	z_step_state:SendCmd("#1D")
	z_step_state:SendCmd("#1y1")
	z_step_state:SendCmd("#1A")
end

function z_go_up_station_cmd()
	z_step_state:SendCmd("#1y2")
	z_step_state:SendCmd("#1o=100")
	z_step_state:SendCmd("#1A")
end

function z_is_moving()
	return z_step_state.IsMoving
end

function y_home_cmd()
	y_step_state:SendCmd("#1D")
	y_step_state:SendCmd("#1y1")
	y_step_state:SendCmd("#1A")
end

function y_go_papar_station_cmd()
	y_step_state:SendCmd("#1y3")
	y_step_state:SendCmd("#1o=100")
	y_step_state:SendCmd("#1A")
end

function y_is_moving()
	return y_step_state.IsMoving
end