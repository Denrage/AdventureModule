-- Gamli
-- Robb
-- Nathan
-- Natasha

key = false;

function onEmote(emote)
    if nathanCuboid:CharacterInside then
        nathanDialog.Show();
    end
    if cabinetCuboid:CharacterInside then
        cabinetDialog.Show();
    end
end

function cabinetConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "In der Vitrine ist ein Fach mit dem Namen \"Nathan\". In diesem befindet sich ein großer Schlüssel");
    dialog.AddEdge(1, -1, "Schließen", nil, function() key = true end);
    cabinetDialog = dialog.Build();
end

function nathanConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "*Nathan versucht zu fliehen, Ihr konntet ihn gerade so noch festhalten.* Ja, ich hab den Punch vergiftet. Ein Elonier kam zu mir und bedrohte mich und meine Familie, ich habe aus Angst gehandelt. Ich wusste nicht, dass Ixea das Ziel sein sollte!");
    -- dialog.AddEdge(2, 1, "Gut", function() : bool return true end);
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    dialog.AddEdge(1, 2, "Ich habe den Lagerraumschlüssel in deinem Fach gefunden.", function() : bool return key end);
    nathanDialog = dialog.Build();
end


function onStart(creator)
    nathanConversation();
    cabinetConversation();
    nathanCuboid = creator:CreateCuboid("Nathan", CreateVector(-91, -453, 19), CreateVector(-2,2,2), 1462);
    cabinetCuboid = creator:CreateCuboid("Cabinet", CreateVector(-91.5, -445, 19), CreateVector(-2,2,2), 1462);
    Character.EmoteUsed:add(onEmote);
end

function onUnload()
    Character.EmoteUsed:remove(onEmote);
end