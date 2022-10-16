function buttonLogicFinished()
    StepLogic.NextStep("Step3");
end

function buttonPressChanged(buttons)
    if buttons:Length > 0 then
        buttons[buttons:Length - 1]:ChangeTexture("marker.png");
    else
        button1:ChangeTexture("button.png");
        button2:ChangeTexture("button.png");
        button3:ChangeTexture("button.png");
    end
end

function onStart(creator)
    button1 = creator:CreateMarker("button1", "button.png", CreateVector(23.88107,-289.9126,35.43097), CreateVector(90,0,180), 1155);
    button2 = creator:CreateMarker("button2", "button.png", CreateVector(4.48517,-300.5934,35.18379), CreateVector(90,0,75), 1155);
    button3 = creator:CreateMarker("button3", "button.png", CreateVector(21.91974,-326.3877,35.18382), CreateVector(90,0,-30), 1155);
    buttonLogicBuilder = LogicCreator:CreateButtonOrderLogic();
    buttonLogicBuilder.Finished:add(buttonLogicFinished);
    buttonLogicBuilder.StateChanged:add(buttonPressChanged);
    buttonLogicBuilder:Add(button1, 0):Add(button2, 0):Add(button3, 0);
    buttonLogicBuilder:Build();
end

function update()
end

function onUnload()
end
