function onBookInteracted()
-- open image window with image
end

function onStart(creator)
    book = creator:CreateMarker("book", CreateVector(23.88107,-289.9126,35.43097), CreateVector(90,0,180), 1155);
    book.Interacted:add(onBookInteracted);
end

function update()
end

function onUnload()
end
