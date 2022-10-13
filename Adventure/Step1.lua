function onEmote(emote)
    if cuboid:CharacterInside then
        StepLogic.NextStep("Step2");
    end
end

function onStart(creator)
    cuboid = creator:CreateCuboid("KneelAreaCuboid", CreateVector(33.97171,-319.3138, 32.18883), CreateVector(-10,10,10), 1155);
    Character.EmoteUsed:add(onEmote);
end

function update()
end

