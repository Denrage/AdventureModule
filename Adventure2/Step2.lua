-- Gamli
-- Robb
-- Nathan
-- Natasha

gamliKey = false;

function onEmote(emote)
    if gamliCuboid:CharacterInside then
        gamliDialog.Show();
    end
    if nathanCuboid:CharacterInside then
        nathanDialog.Show();
    end
    if natashaCuboid:CharacterInside then
        natashaDialog.Show();
    end
    if cabinetCuboid:CharacterInside then
        cabinetDialog.Show();
    end
end

function cabinetConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Die Vitrine scheint verschlossen zu sein. Natasha macht einen darauf aufmerksam, dass Gamli den Schlüssel hinter der Theke aufbewahrt.");
    dialog.AddEdge(1, -1, "Schließen", nil, function() gamliKey = true end);
    cabinetDialog = dialog.Build();
end

function gamliConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Ich erinnere mich nicht an solch eine Teekanne, diese gehört hier definitiv nicht. Unfassbar, dass jemand in meiner Schänke vergiftet wurde und dann auch noch Offizierin Ixea. Ich bin ratlos, wer dafür verantwortlich sein könne, aber alle meine Mitarbeiter vertraue ich wie der eigenen Familie.");
    dialog.AddNode(3, "Kein Problem, hier ist der Schlüssel.")
    dialog.AddEdge(1, 2, "Ich habe diese Teekanne gefunden.");
    dialog.AddEdge(1, 3, "Könnte ich den Schlüssel für die Vitrine bekommen?", function() : bool return gamliKey end);
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    dialog.AddEdge(3, -1, "Auf Wiedersehen", nil, function() StepLogic.NextStep("Step3") end);
    gamliDialog = dialog.Build();
end

function nathanConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Ehhh, also, diese Kanne, da, die habe ich noch nie gesehen, wirklich. Ich habe nur den Punch im Laggerraum deponiert, ich weiß nicht, wie lange die da schon ist. Ich weiß, dass mich das verdächtigt macht, aber ich habe damit wirklich nichts zu tun!");
    -- dialog.AddEdge(2, 1, "Gut", function() : bool return true end);
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    dialog.AddEdge(1, 2, "War diese Kanne schon im Lagerraum, als du den Punch deponiert hattest?");
    nathanDialog = dialog.Build();
end

function natashaConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Diese Teekanne habe ich noch nie gesehen. Nathan hatte sich aber dahinten an der Vitrine nervös und seltsam verhalten. Wir bewahren da unsere Gegenstände auf, während wir arbeiten, er selbst ist aber noch mitten in seiner Schicht und hat eigentlich keinen Grund, an die Vitrine zu gehen. Er hat sich durchgängig umgeschaut, um sicherzugehen, dass er nicht beobachtet wird.");
    dialog.AddEdge(1, 2, "Hast du diese Teekanne jemals gesehen?");
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    natashaDialog = dialog.Build();
end

function onStart(creator)
    gamliConversation();
    nathanConversation();
    natashaConversation();
    cabinetConversation();
    gamliCuboid = creator:CreateCuboid("Gamli", CreateVector(-79, -446, 19), CreateVector(-2,2,2), 1462);
    nathanCuboid = creator:CreateCuboid("Nathan", CreateVector(-91, -453, 19), CreateVector(-2,2,2), 1462);
    natashaCuboid = creator:CreateCuboid("Natasha", CreateVector(-90, -438, 19), CreateVector(-2,2,2), 1462);
    cabinetCuboid = creator:CreateCuboid("Cabinet", CreateVector(-91.5, -445, 19), CreateVector(-2,2,2), 1462);
    Character.EmoteUsed:add(onEmote);
end

function onUnload()
    Character.EmoteUsed:remove(onEmote);
end