'use client';

import { PromptTemplate, Category, Tag } from '@/types';
import { Edit, Trash2, Play, Calendar, Tag as TagIcon, Folder } from 'lucide-react';

interface PromptListProps {
  prompts: PromptTemplate[];
  categories: Category[];
  tags: Tag[];
  onEdit: (prompt: PromptTemplate) => void;
  onDelete: (id: string) => void;
  onExecute: (prompt: PromptTemplate) => void;
}

export default function PromptList({
  prompts,
  categories,
  tags,
  onEdit,
  onDelete,
  onExecute,
}: PromptListProps) {
  const getTagNames = (tagIds: string[]) => {
    return tagIds.map(tagId => tags.find(t => t.id === tagId)?.name).filter(Boolean);
  };

  const formatDate = (date: Date) => {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  if (prompts.length === 0) {
    return (
      <div className="bg-white shadow-lg p-16 rounded-xl text-center">
        <div className="mb-6 text-6xl">🚀</div>
        <h3 className="mb-4 font-bold text-gray-900 text-2xl">Ready to Create Magic?</h3>
        <p className="mx-auto max-w-lg text-gray-600 text-lg leading-relaxed">
          Your prompt workspace is empty, but full of potential! Create your first prompt template 
          and start building powerful AI workflows.
        </p>
      </div>
    );
  }

  return (    <div className="gap-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
      {prompts.map((prompt) => {
        const category = categories.find(c => c.id === prompt.categoryId);
        return (          <div
            key={prompt.id}
            className="group bg-white shadow-lg hover:shadow-xl border border-gray-200 hover:border-indigo-300 rounded-xl overflow-hidden hover:scale-[1.02] transition-all duration-300 cursor-pointer"
            onClick={() => onEdit(prompt)}
          >
            {/* Category Color Bar */}
            {category && (
              <div 
                className="h-2"
                style={{ backgroundColor: category.color || '#6b7280' }}
              />
            )}
            
            <div className="space-y-4 p-6">
              {/* Header Section */}
              <div className="space-y-3">
                <div className="flex justify-between items-start">
                  <h3 className="font-bold text-gray-900 text-lg leading-tight">
                    {prompt.name}
                  </h3>                  <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onExecute(prompt);
                      }}
                      className="hover:bg-green-100 p-2 rounded-lg text-green-600 transition-colors"
                      title="Execute prompt"
                    >
                      <Play className="w-4 h-4" />
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onEdit(prompt);
                      }}
                      className="hover:bg-blue-100 p-2 rounded-lg text-blue-600 transition-colors"
                      title="Edit prompt"
                    >
                      <Edit className="w-4 h-4" />
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onDelete(prompt.id);
                      }}
                      className="hover:bg-red-100 p-2 rounded-lg text-red-600 transition-colors"
                      title="Delete prompt"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>

                {/* Description */}
                {prompt.description && (
                  <p className="text-gray-600 text-sm line-clamp-2 leading-relaxed">
                    {prompt.description}
                  </p>
                )}
              </div>

              {/* Content Preview */}
              <div className="bg-gray-50 p-3 border rounded-lg">
                <p className="font-mono text-gray-700 text-sm line-clamp-3 leading-relaxed">
                  {prompt.content}
                </p>
              </div>

              {/* Metadata */}
              <div className="space-y-3">
                {/* Category and Variables */}
                <div className="flex flex-wrap items-center gap-2">
                  {category && (
                    <div className="flex items-center gap-2 bg-gray-100 px-3 py-1 rounded-full">
                      <div 
                        className="rounded-full w-2 h-2"
                        style={{ backgroundColor: category.color || '#6b7280' }}
                      />
                      <Folder className="w-3 h-3 text-gray-500" />
                      <span className="font-medium text-gray-700 text-xs">
                        {category.name}
                      </span>
                    </div>
                  )}

                  {prompt.variables.length > 0 && (
                    <div className="bg-blue-50 px-3 py-1 rounded-full">
                      <span className="font-medium text-blue-700 text-xs">
                        {prompt.variables.length} variable{prompt.variables.length !== 1 ? 's' : ''}
                      </span>
                    </div>
                  )}
                </div>

                {/* Tags */}
                {prompt.tags.length > 0 && (
                  <div className="space-y-2">
                    <div className="flex items-center gap-2">
                      <TagIcon className="w-4 h-4 text-gray-400" />
                      <div className="flex flex-wrap gap-1">
                        {getTagNames(prompt.tags).map((tagName) => (
                          <span
                            key={tagName}
                            className="inline-block bg-purple-100 px-2 py-1 rounded-md font-medium text-purple-800 text-xs"
                          >
                            {tagName}
                          </span>
                        ))}
                      </div>
                    </div>
                  </div>
                )}
              </div>              {/* Footer */}
              <div className="pt-3 border-gray-200 border-t">
                <div className="flex justify-between items-center">
                  <div className="flex items-center gap-1 text-gray-500 text-xs">
                    <Calendar className="w-3 h-3" />
                    <span>Updated {formatDate(prompt.updatedAt)}</span>
                  </div>
                  <div className="opacity-0 group-hover:opacity-100 text-gray-400 text-xs transition-opacity">
                    Click to edit
                  </div>
                </div>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
