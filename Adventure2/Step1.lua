-- Gamli
-- Robb
-- Nathan
-- Natasha

function gamliConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    gamliDialog = dialog.Build();
    --gamliDialog.Show();
end

function robbConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Wie geht es dir?");
    dialog.AddEdge(1, 2, "Hallo");
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    robbDialog = dialog.Build();
end

function nathanConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Wie geht es dir?");
    dialog.AddEdge(1, 2, "Hallo");
    dialog.AddEdge(2, 1, "Gut", function() : bool return true end);
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    nathanDialog = dialog.Build();
    nathanDialog.Show();
end

function natashaConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    natashaDialog = dialog.Build();
end

function onStart(creator)
    gamliConversation();
    robbConversation();
    nathanConversation();
    natashaConversation();
    --cuboid = creator:CreateCuboid("KneelAreaCuboid", CreateVector(33.97171,-319.3138, 32.18883), CreateVector(-10,10,10), 1155);
    --Character.EmoteUsed:add(onEmote);
end