import { Injectable } from '@angular/core';
import { Guid } from 'guid-typescript';

@Injectable({
  providedIn: 'root'
})
export class AdventureService {

  groups: AdventureGroup[] = [
    {
      id: Guid.parse("aabb4d15-b681-4040-9e0b-b6d4154bb6d0"), name: "Ainur", image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp", players: [
        Guid.parse("f683dad2-9a68-49ba-b6f5-0d91964607d3"),
        Guid.parse("bd2d7760-fcaf-4246-9324-4d96ab7d85eb"),
        Guid.parse("c59cfe25-cb75-4783-a024-5bd709053a16"),
        Guid.parse("91c6f817-8597-4f54-a506-ec032bdf436c"),
        Guid.parse("f0e8a302-1d1d-45f3-9229-8dc601dd5ecb")
      ], progress: {}
    },
    {
      id: Guid.parse("babb4d15-b681-4040-9e0b-b6d4154bb6d0"), name: "Ainur2", image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp", players: [
        Guid.parse("f683dad2-9a68-49ba-b6f5-0d91964607d3"),
        Guid.parse("bd2d7760-fcaf-4246-9324-4d96ab7d85eb"),
        Guid.parse("c59cfe25-cb75-4783-a024-5bd709053a16"),
        Guid.parse("91c6f817-8597-4f54-a506-ec032bdf436c"),
        Guid.parse("f0e8a302-1d1d-45f3-9229-8dc601dd5ecb")
      ], progress: {}
    },
    {
      id: Guid.parse("cabb4d15-b681-4040-9e0b-b6d4154bb6d0"), name: "Ainur3", image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp", players: [
        Guid.parse("f683dad2-9a68-49ba-b6f5-0d91964607d3"),
        Guid.parse("bd2d7760-fcaf-4246-9324-4d96ab7d85eb"),
        Guid.parse("c59cfe25-cb75-4783-a024-5bd709053a16"),
        Guid.parse("91c6f817-8597-4f54-a506-ec032bdf436c"),
        Guid.parse("f0e8a302-1d1d-45f3-9229-8dc601dd5ecb")
      ], progress: {}
    },
    {
      id: Guid.parse("dabb4d15-b681-4040-9e0b-b6d4154bb6d0"), name: "Ainur4", image: "https://cdn.discordapp.com/avatars/225594944247693323/14042fda7b551ef22107105e533322ba.webp", players: [
        Guid.parse("f683dad2-9a68-49ba-b6f5-0d91964607d3"),
        Guid.parse("bd2d7760-fcaf-4246-9324-4d96ab7d85eb"),
        Guid.parse("c59cfe25-cb75-4783-a024-5bd709053a16"),
        Guid.parse("91c6f817-8597-4f54-a506-ec032bdf436c"),
        Guid.parse("f0e8a302-1d1d-45f3-9229-8dc601dd5ecb")
      ], progress: {}
    },
  ]

  adventures: Adventure[] = [
    {
      id: Guid.parse("310e0336-cc92-49dd-a38b-88ddff1595e3"),
      templateId: Guid.create(),
      name: "FooBar",
      groups: [Guid.parse("aabb4d15-b681-4040-9e0b-b6d4154bb6d0"), Guid.parse("babb4d15-b681-4040-9e0b-b6d4154bb6d0"), Guid.parse("cabb4d15-b681-4040-9e0b-b6d4154bb6d0"), Guid.parse("dabb4d15-b681-4040-9e0b-b6d4154bb6d0")]
    },
    {
      id: Guid.parse("410e0336-cc92-49dd-a38b-88ddff1595e3"),
      templateId: Guid.create(),
      name: "HelloWorld",
      groups: [Guid.parse("aabb4d15-b681-4040-9e0b-b6d4154bb6d0"), Guid.parse("babb4d15-b681-4040-9e0b-b6d4154bb6d0"), Guid.parse("cabb4d15-b681-4040-9e0b-b6d4154bb6d0"), Guid.parse("dabb4d15-b681-4040-9e0b-b6d4154bb6d0")]
    }];

  constructor() { }

  getAdventures(): Adventure[] {
    return this.adventures;
  }

  getAdventure(adventureId: Guid): Adventure {
    return this.adventures.filter(adventure => adventure.id.equals(adventureId))[0];
  } 

  getAdventureGroup(groupId: Guid): AdventureGroup {
    return this.groups.filter(group => group.id.equals(groupId))[0];
  } 
}

export interface Adventure {
  id: Guid;
  templateId: Guid;
  name: string;
  groups: Guid[];
}

export interface AdventureGroup {
  id: Guid;
  name: string;
  progress: AdventureProgess;
  image: string;
  players: Guid[];
}

export interface AdventureProgess {
}
