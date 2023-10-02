import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavigationComponent } from './navigation/navigation.component';
import { ParticipantsComponent } from './admin/participants/participants.component';
import { AdventuresComponent } from './admin/adventures/adventures.component';
import { AdventureDetailComponent } from './admin/adventures/adventure-detail/adventure-detail.component';

import { ButtonModule } from 'primeng/button'
import { PanelMenuModule } from 'primeng/panelmenu';
import { CardModule } from 'primeng/card';

@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,
    ParticipantsComponent,
    AdventuresComponent,
    AdventureDetailComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    ButtonModule,
    PanelMenuModule,
    CardModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
