import { Injectable } from '@angular/core';
import { Adventure } from '../models/adventure';

@Injectable({
  providedIn: 'root'
})
export class AdventureService {

  constructor() { }

  public getAdventures(): Adventure[] {
    return [
      { name: "HelloWorld1", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld2", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld3", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld4", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld5", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld6", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld7", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld8", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld9", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld10", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld11", picture: "https://picsum.photos/200/100", groups: [] },
      { name: "HelloWorld12", picture: "https://picsum.photos/200/100", groups: [] },
    ]
  }
}
