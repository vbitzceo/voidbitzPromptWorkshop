'use client';

import { PromptTemplate, Category, Tag } from '@/types';
import { FileText, Folder, Hash, Clock } from 'lucide-react';

interface StatsProps {
  prompts: PromptTemplate[];
  categories: Category[];
  tags: Tag[];
  hideRecentActivity?: boolean;
}

export default function StatsDashboard({ prompts, categories, tags, hideRecentActivity = false }: StatsProps) {
  const recentPrompts = prompts
    .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
    .slice(0, 3);

  const stats = [
    {
      label: 'Total Prompts',
      value: prompts.length,
      icon: FileText,
      color: 'from-blue-500 to-blue-600',
      bgColor: 'bg-blue-50',
      textColor: 'text-blue-700',
    },
    {
      label: 'Categories',
      value: categories.length,
      icon: Folder,
      color: 'from-green-500 to-green-600',
      bgColor: 'bg-green-50',
      textColor: 'text-green-700',
    },
    {
      label: 'Tags',
      value: tags.length,
      icon: Hash,
      color: 'from-purple-500 to-purple-600',
      bgColor: 'bg-purple-50',
      textColor: 'text-purple-700',
    },
    {
      label: 'Variables',
      value: prompts.reduce((total, prompt) => total + prompt.variables.length, 0),
      icon: Clock,
      color: 'from-orange-500 to-orange-600',
      bgColor: 'bg-orange-50',
      textColor: 'text-orange-700',
    },
  ];

  return (
    <div className="mb-8">
      {/* Stats Cards */}
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 mb-6">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div
              key={stat.label}
              className="bg-white shadow-lg hover:shadow-xl p-6 border border-gray-200 rounded-xl transition-shadow duration-300"
            >
              <div className="flex justify-between items-center">
                <div>
                  <p className="mb-1 font-medium text-gray-600 text-sm">{stat.label}</p>
                  <p className="font-bold text-gray-900 text-3xl">{stat.value}</p>
                </div>
                <div className={`${stat.bgColor} p-3 rounded-lg`}>
                  <Icon className={`w-6 h-6 ${stat.textColor}`} />
                </div>
              </div>
            </div>
          );
        })}
      </div>      {/* Recent Activity */}
      {!hideRecentActivity && recentPrompts.length > 0 && (
        <div className="bg-white shadow-lg p-6 border border-gray-200 rounded-xl">
          <h3 className="flex items-center gap-2 mb-4 font-semibold text-gray-900 text-lg">
            <Clock className="w-5 h-5 text-gray-500" />
            Recent Activity
          </h3>
          <div className="space-y-3">
            {recentPrompts.map((prompt) => {
              const category = categories.find(c => c.id === prompt.categoryId);
              return (
                <div key={prompt.id} className="flex items-center gap-3 bg-gray-50 p-3 rounded-lg">
                  {category && (
                    <div
                      className="rounded-full w-3 h-3"
                      style={{ backgroundColor: category.color }}
                    />
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="font-medium text-gray-900 truncate">{prompt.name}</p>
                    <p className="text-gray-500 text-sm truncate">{prompt.description}</p>
                  </div>
                  <div className="text-gray-400 text-xs">
                    {new Date(prompt.updatedAt).toLocaleDateString()}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
