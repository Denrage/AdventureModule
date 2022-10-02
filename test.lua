function buttonLogicFinished()
    cuboid:Test();
end

function buttonPressChanged(buttons)
    if buttons:Length > 0 then
        for i = 0, buttons:Length - 1 do
            buttons[i]:FlipNinetyDegrees();
        end
    end
end

function onStart(creator)
    cuboid = creator:CreateCuboid("test", CreateVector(33.97171,-319.3138, 32.18883), CreateVector(-10,10,10));
    button1 = creator:CreateMarker("button1", CreateVector(23.88107,-289.9126,35.43097), CreateVector(90,0,180));
    button2 = creator:CreateMarker("button2", CreateVector(4.48517,-300.5934,35.18379), CreateVector(90,0,75));
    button3 = creator:CreateMarker("button3", CreateVector(21.91974,-326.3877,35.18382), CreateVector(90,0,-30));
    buttonLogicBuilder = LogicCreator:CreateButtonOrderLogic();
    buttonLogicBuilder.Finished:add(buttonLogicFinished);
    buttonLogicBuilder.StateChanged:add(buttonPressChanged);
    buttonLogicBuilder:Add(button1, 0):Add(button2, 0):Add(button3, 0);
    buttonLogicBuilder:Build();
end

function update()
end

-- xpos="23.88107" ypos="34.43097" zpos="-290.6126"
-- xpos="5.38517" ypos="34.18379" zpos="-300.5934"
-- xpos="21.91974" ypos="34.18382" zpos="-325.3877"

function onEmote(emote)
    if cuboid:CharacterInside then
        cuboid:Test();
    end
end

function onStarted()
    Character.EmoteUsed:add(onEmote);
end