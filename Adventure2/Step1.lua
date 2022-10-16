-- Gamli
-- Robb
-- Nathan
-- Natasha

ServerVariables.interactedWithWall = false;
ServerVariables.interactedWithTablet = false;
ServerVariables.talkedWithGamliKey = false;

function onEmote(emote)
    if robbCuboid:CharacterInside then
        robbDialog.Show();
    end
    if gamliCuboid:CharacterInside then
        gamliDialog.Show();
    end
    if nathanCuboid:CharacterInside then
        nathanDialog.Show();
    end
    if natashaCuboid:CharacterInside then
        natashaDialog.Show();
    end
    if tabletPoisonedCuboid:CharacterInside then
        poisonedTabletDialog.Show();
    end
    if tabletCuboid:CharacterInside then
        tabletDialog.Show();
    end
    if wallCuboid:CharacterInside then
        wallDialog.Show();
    end
    if teapotCuboid:CharacterInside then
        teapotDialog.Show();
    end
end

function poisonedTabletConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Dies scheint ein normaler Eierpunch zu sein, aber bei näherer Betrachtung blubbert dieser und verströmt einen unerwarteten Geruch.");
    dialog.AddEdge(1, -1, "Schließen", nil, function() ServerVariables.interactedWithTablet = true end);
    poisonedTabletDialog = dialog.Build();
end

function tabletConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Dies scheint ein normaler Eierpunch zu sein.");
    dialog.AddEdge(1, -1, "Schließen");
    tabletDialog = dialog.Build();
end

function wallConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Diese Wand scheint neu errichtet worden zu sein. Durch die Dekorationen wurde diese sehr gut in den Raum integriert.");
    dialog.AddEdge(1, -1, "Schließen", nil, function() ServerVariables.interactedWithWall = true end);
    wallDialog = dialog.Build();
end

function teapotConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Diese Teekanne und der Tisch sehen nach elonischem Ursprung aus und passen nicht zu den restlichen Design dieser Schänke. Beim Öffnen der Teekanne verströmt diese einen ähnlichen Duft wie die in dem Eierpunch am Thron.");
    dialog.AddEdge(1, -1, "Teekanne mitnehmen", nil, function() StepLogic.NextStep("Step2") end);
    teapotDialog = dialog.Build();
end

function gamliConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Der Raum hinter der Wand ist lediglich ein Lagerraum, bspw. nicht genutzte Theken oder Getränke");
    dialog.AddNode(3, "Ein Lieferant hatte diesen vorhin gebracht. Nathan nahm ihn an und deponierte diesen im Lagerraum. Nathan hat auch den Schlüssel für diesen.");
    dialog.AddEdge(1, 2, "Was befindet sich hinter der Wand dahinten?", function() : bool return ServerVariables.interactedWithWall end);
    dialog.AddEdge(1, 3, "Woher kommt der Punch?", function() : bool return ServerVariables.interactedWithTablet end);
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    dialog.AddEdge(2, 1, "Zurück");
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    dialog.AddEdge(3, -1, "Auf Wiedersehen", nil, function() ServerVariables.talkedWithGamliKey = true end);
    gamliDialog = dialog.Build();
    --gamliDialog.Show();
end

function robbConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Ixea hatte diese beiden Tablette geholt, bevor sie gestorben ist. Da ich gerade meine Flöte ausgepackt hatte, sah ich nicht, was passiert ist, als sie umgefallen ist.");
    dialog.AddEdge(1, 2, "Woher kommt der Eierpunsch?", function() : bool return ServerVariables.interactedWithTablet end);
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    robbDialog = dialog.Build();
end

function nathanConversation()
    local dialog = Dialog:Create();
    dialog.AddNode(1, "Hallo");
    dialog.AddNode(2, "Ich hatte den Punch angenommen und im Lagerraum deponiert.");
    dialog.AddNode(3, "Ich kann den Schlüssel gerade nicht finden, dabei hatte ich ihn gerade noch irgendwo.");
    -- dialog.AddEdge(2, 1, "Gut", function() : bool return true end);
    dialog.AddEdge(1, 2, "Was hattest du mit dem Punch gemacht?", function() : bool return ServerVariables.interactedWithTablet end);
    dialog.AddEdge(1, 3, "Gamli meinte, du hättest den Schlüssel für den Laggerraum, könnte ich diesen haben?", function() : bool return ServerVariables.talkedWithGamliKey end);
    dialog.AddEdge(1, -1, "Auf Wiedersehen");
    dialog.AddEdge(2, -1, "Auf Wiedersehen");
    dialog.AddEdge(3, -1, "Auf Wiedersehen");
    nathanDialog = dialog.Build();
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
    poisonedTabletConversation();
    tabletConversation();
    wallConversation();
    teapotConversation();
    robbCuboid = creator:CreateCuboid("Robb", CreateVector(-71, -438, 19), CreateVector(-2,2,2), 1462);
    gamliCuboid = creator:CreateCuboid("Gamli", CreateVector(-79, -446, 19), CreateVector(-2,2,2), 1462);
    nathanCuboid = creator:CreateCuboid("Nathan", CreateVector(-91, -453, 19), CreateVector(-2,2,2), 1462);
    natashaCuboid = creator:CreateCuboid("Natasha", CreateVector(-90, -438, 19), CreateVector(-2,2,2), 1462);
    tabletPoisonedCuboid = creator:CreateCuboid("TabletPoisoned", CreateVector(-65, -441, 19), CreateVector(-2,2,2), 1462);
    tabletCuboid = creator:CreateCuboid("Tablet", CreateVector(-72, -445, 19), CreateVector(-2,2,2), 1462);
    wallCuboid = creator:CreateCuboid("Wall", CreateVector(-71, -451, 19), CreateVector(-2,2,2), 1462);
    teapotCuboid = creator:CreateCuboid("Teapot", CreateVector(-64, -454, 19), CreateVector(-2,2,2), 1462);
    Character.EmoteUsed:add(onEmote);
end

function onUnload()
    Character.EmoteUsed:remove(onEmote);
end