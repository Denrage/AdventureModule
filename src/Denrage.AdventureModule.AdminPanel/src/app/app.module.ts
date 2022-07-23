import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { PlayerComponent } from './player/player.component';
import { AdventureGroupComponent } from './adventure-group/adventure-group.component';
import { AdventureComponent } from './adventure/adventure.component';
import { AdventureListComponent } from './adventure-list/adventure-list.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';

@NgModule({
  declarations: [
    AppComponent,
    PlayerComponent,
    AdventureGroupComponent,
    AdventureComponent,
    AdventureListComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatCardModule,
    MatButtonModule,
    MatListModule,
    MatGridListModule,
    MatIconModule,
    MatSidenavModule,
    MatToolbarModule,
    RouterModule.forRoot([
      { path: '', component: AdventureListComponent },
      { path: 'adventures', component: AdventureListComponent },
      { path: 'adventures/:id', component: AdventureComponent}
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
