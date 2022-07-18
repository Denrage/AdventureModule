import { Component, OnInit } from '@angular/core';
import { Adventure, AdventureService } from '../adventure.service';

@Component({
  selector: 'app-adventure-list',
  templateUrl: './adventure-list.component.html',
  styleUrls: ['./adventure-list.component.css']
})
export class AdventureListComponent implements OnInit {

  adventures: Adventure[] = [];

  constructor(private adventureService: AdventureService) { }

  ngOnInit(): void {
    this.adventures = this.adventureService.getAdventures();
  }

}
