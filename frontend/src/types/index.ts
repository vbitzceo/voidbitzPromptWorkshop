export interface PromptTemplate {
  id: string;
  name: string;
  description: string;
  content: string;
  yamlTemplate: string;
  categoryId?: string;
  tags: string[];
  variables: PromptVariable[];
  createdAt: Date;
  updatedAt: Date;
}

export interface PromptVariable {
  name: string;
  description: string;
  type: 'string' | 'number' | 'boolean';
  required: boolean;
  defaultValue?: string;
}

export interface Category {
  id: string;
  name: string;
  description: string;
  color: string;
  createdAt: Date;
}

export interface Tag {
  id: string;
  name: string;
  description: string;
  color: string;
}

export interface PromptExecution {
  id: string;
  promptTemplateId: string;
  variables: Record<string, unknown>;
  result: string;
  executedAt: Date;
}

export interface CreatePromptTemplateRequest {
  name: string;
  description: string;
  content: string;
  categoryId?: string;
  tags: string[];
  variables: PromptVariable[];
}

export interface UpdatePromptTemplateRequest extends Partial<CreatePromptTemplateRequest> {
  id: string;
}

export interface ExecutePromptRequest {
  promptTemplateId: string;
  variables: Record<string, unknown>;
}

export interface PromptSuggestionRequest {
  name: string;
  content: string;
}

export interface PromptSuggestionResponse {
  suggestedCategoryId?: string;
  suggestedTagIds: string[];
  reasoning: string;
}
