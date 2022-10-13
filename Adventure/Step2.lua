-- possibility to set and change texture
-- only display if a certain distance next to it
function buttonLogicFinished()
    StepLogic.NextStep("Step3");
end

function buttonPressChanged(buttons)
    if buttons:Length > 0 then
        for i = 0, buttons:Length - 1 do
            buttons[i]:FlipNinetyDegrees();
        end
    end
end

function onStart(creator)
    button1 = creator:CreateMarker("button1", CreateVector(23.88107,-289.9126,35.43097), CreateVector(90,0,180), 1155);
    button2 = creator:CreateMarker("button2", CreateVector(4.48517,-300.5934,35.18379), CreateVector(90,0,75), 1155);
    button3 = creator:CreateMarker("button3", CreateVector(21.91974,-326.3877,35.18382), CreateVector(90,0,-30), 1155);
    buttonLogicBuilder = LogicCreator:CreateButtonOrderLogic();
    buttonLogicBuilder.Finished:add(buttonLogicFinished);
    buttonLogicBuilder.StateChanged:add(buttonPressChanged);
    buttonLogicBuilder:Add(button1, 0):Add(button2, 0):Add(button3, 0);
    buttonLogicBuilder:Build();
end

function update()
end