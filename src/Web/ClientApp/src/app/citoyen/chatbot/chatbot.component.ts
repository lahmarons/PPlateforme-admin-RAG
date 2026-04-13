import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { TagModule } from 'primeng/tag';

type ChatMessage = { from: 'user' | 'bot'; text: string };

@Component({
  selector: 'app-citoyen-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, TextareaModule, TagModule],
  templateUrl: './chatbot.component.html'
})
export class ChatbotComponent {
  question = '';
  messages: ChatMessage[] = [
    { from: 'bot', text: 'Bonjour 👋 Je suis votre assistant citoyen. Comment puis-je vous aider ?' }
  ];

  send(): void {
    const text = this.question.trim();
    if (!text) {
      return;
    }

    this.messages.push({ from: 'user', text });
    this.question = '';

    const lower = text.toLowerCase();
    const answer = lower.includes('demande')
      ? 'Pour créer une demande, allez dans "Services" ou "Créer une demande" depuis votre tableau de bord.'
      : lower.includes('service')
      ? 'Vous pouvez consulter les services disponibles dans la rubrique "Services".'
      : 'Merci pour votre message. Un agent pourra vous assister davantage si nécessaire.';

    this.messages.push({ from: 'bot', text: answer });
  }
}
