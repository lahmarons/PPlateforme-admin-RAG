import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';

type AgentChatMessage = { from: 'agent' | 'bot'; text: string };

@Component({
  selector: 'app-agent-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, TagModule, TextareaModule],
  templateUrl: './agent-chatbot.component.html'
})
export class AgentChatbotComponent {
  question = '';
  messages: AgentChatMessage[] = [
    { from: 'bot', text: 'Bonjour agent 👋 Posez une question sur le traitement des demandes.' }
  ];

  send(): void {
    const text = this.question.trim();
    if (!text) {
      return;
    }

    this.messages.push({ from: 'agent', text });
    this.question = '';

    const lower = text.toLowerCase();
    const answer = lower.includes('refus')
      ? 'Vous pouvez refuser la demande depuis la liste des demandes via le bouton "Refuser".'
      : lower.includes('valider') || lower.includes('traiter')
      ? 'Pour valider, utilisez le bouton "Valider" afin de passer le statut à "Traitée".'
      : 'Action suggérée: consulter les demandes puis prendre une décision (valider/refuser).';

    this.messages.push({ from: 'bot', text: answer });
  }
}
