'use client';

import { useState, useEffect, useMemo } from 'react';
import { Plus, Search, HelpCircle, X } from 'lucide-react';
import { PromptTemplate, Category, Tag } from '@/types';
import { promptTemplateAPI, categoryAPI, tagAPI } from '@/services/api';
import PromptEditor from '@/components/PromptEditor';
import PromptList from '@/components/PromptList';
import CategoryManager from '@/components/CategoryManager';
import TagManager from '@/components/TagManager';
import StatsDashboard from '@/components/StatsDashboard';
import LoadingSpinner from '@/components/LoadingSpinner';
import WelcomeTour from '@/components/WelcomeTour';
import toast from 'react-hot-toast';

export default function Home() {
  const [prompts, setPrompts] = useState<PromptTemplate[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [tags, setTags] = useState<Tag[]>([]);
  const [selectedPrompt, setSelectedPrompt] = useState<PromptTemplate | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [showEditor, setShowEditor] = useState(false);
  const [showCategoryManager, setShowCategoryManager] = useState(false);
  const [showTagManager, setShowTagManager] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [showWelcomeTour, setShowWelcomeTour] = useState(false);

  useEffect(() => {
    loadData();
    const hasSeenTour = localStorage.getItem('hasSeenTour');
    if (!hasSeenTour) {
      setTimeout(() => setShowWelcomeTour(true), 1000);
    }
  }, []);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [promptsData, categoriesData, tagsData] = await Promise.all([
        promptTemplateAPI.getAll(),
        categoryAPI.getAll(),
        tagAPI.getAll(),
      ]);
      setPrompts(promptsData);
      setCategories(categoriesData);
      setTags(tagsData);
    } catch (error) {
      toast.error('Failed to load data');
      console.error('Error loading data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSavePrompt = async (prompt: PromptTemplate) => {
    try {
      let savedPrompt: PromptTemplate;
      if (prompt.id) {
        savedPrompt = await promptTemplateAPI.update(prompt);
        setPrompts(prompts.map(p => p.id === prompt.id ? savedPrompt : p));
        toast.success('Prompt updated successfully');
      } else {
        savedPrompt = await promptTemplateAPI.create(prompt);
        setPrompts([...prompts, savedPrompt]);
        toast.success('Prompt created successfully');
      }
      setShowEditor(false);
    } catch (error) {
      toast.error('Failed to save prompt');
      console.error('Error saving prompt:', error);
    }
  };

  const handleDeletePrompt = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this prompt?')) {
      try {
        await promptTemplateAPI.delete(id);
        setPrompts(prompts.filter(p => p.id !== id));
        toast.success('Prompt deleted successfully');
      } catch (error) {
        toast.error('Failed to delete prompt');
        console.error('Error deleting prompt:', error);
      }
    }
  };

  const handleExecutePrompt = async (prompt: PromptTemplate, variables: Record<string, unknown>) => {
    try {
      const execution = await promptTemplateAPI.execute({
        promptTemplateId: prompt.id,
        variables,
      });
      toast.success('Prompt executed successfully');
      return execution.result;
    } catch (error) {
      toast.error('Failed to execute prompt');
      console.error('Error executing prompt:', error);
      return null;
    }
  };  // Enhanced filtering with better search capabilities
  const filteredPrompts = useMemo(() => {
    if (!searchTerm && !selectedCategory && selectedTags.length === 0) {
      return prompts;
    }

    return prompts.filter(prompt => {
      // Enhanced search that includes content and variable names
      const searchLower = searchTerm.toLowerCase().trim();
      const matchesSearch = !searchTerm || 
        prompt.name.toLowerCase().includes(searchLower) ||
        prompt.description.toLowerCase().includes(searchLower) ||
        prompt.content.toLowerCase().includes(searchLower) ||
        prompt.variables.some(variable => 
          variable.name.toLowerCase().includes(searchLower) ||
          variable.description.toLowerCase().includes(searchLower)
        );
      
      const matchesCategory = !selectedCategory || prompt.categoryId === selectedCategory;
      const matchesTags = selectedTags.length === 0 || 
                         selectedTags.every(tag => prompt.tags.includes(tag));
      
      return matchesSearch && matchesCategory && matchesTags;
    });
  }, [prompts, searchTerm, selectedCategory, selectedTags]);

  // Handle keyboard shortcuts for search
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      // Ctrl+K or Cmd+K to focus search
      if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
        event.preventDefault();
        const searchInput = document.querySelector('input[placeholder*="Search"]') as HTMLInputElement;
        if (searchInput) {
          searchInput.focus();
          searchInput.select();
        }
      }
      // Escape to clear search
      if (event.key === 'Escape' && searchTerm) {
        setSearchTerm('');
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [searchTerm]);

  if (showEditor) {
    return (
      <PromptEditor
        prompt={selectedPrompt}
        categories={categories}
        tags={tags}
        onSave={handleSavePrompt}
        onCancel={() => setShowEditor(false)}
        onExecute={handleExecutePrompt}
      />
    );
  }

  return (
    <div className="min-h-screen" style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
      <div className="mx-auto px-6 py-10 max-w-7xl container">
          {/* Clean Header */}
        <div className="mb-12 text-center">
          <h1 className="mb-4 font-bold text-white text-5xl">
            voidBitz Prompt Workshop
          </h1>
          <p className="opacity-90 mx-auto max-w-2xl text-white text-xl">
            Create, manage, and execute LLM prompt templates with ease
          </p>
        </div>

        {/* Action Bar with Better Spacing */}
        <div className="bg-white shadow-lg mb-8 p-8 rounded-xl">
          
          {/* Search and New Button Row */}
          <div className="gap-4 grid grid-cols-1 lg:grid-cols-12 mb-6">            <div className="lg:col-span-8">
              <div className="relative">
                <Search className="top-1/2 left-3 absolute w-5 h-5 text-gray-400 -translate-y-1/2 transform" />                <input
                  type="text"
                  placeholder="Search prompts by name, description, content, or variables... (Ctrl+K)"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="py-3 pr-12 pl-10 border-2 border-gray-200 focus:border-indigo-500 rounded-lg focus:outline-none w-full text-gray-800 transition-colors"
                  style={{ fontSize: '16px' }}
                  autoComplete="off"
                  spellCheck="false"
                />
                {searchTerm && (
                  <button
                    onClick={() => setSearchTerm('')}
                    className="top-1/2 right-3 absolute p-1 text-gray-400 hover:text-gray-600 transition-colors -translate-y-1/2 transform"
                    title="Clear search (Esc)"
                    type="button"
                  >
                    <X className="w-4 h-4" />
                  </button>
                )}
              </div>              {searchTerm && (
                <div className="flex justify-between items-center mt-2 text-sm">
                  <span className="text-gray-600">
                    Found {filteredPrompts.length} prompt{filteredPrompts.length !== 1 ? 's' : ''} matching &quot;{searchTerm}&quot;
                  </span>
                  <span className="text-gray-400 text-xs">
                    Press Esc to clear
                  </span>
                </div>
              )}
            </div>
            <div className="lg:col-span-4">
              <button
                onClick={() => {
                  setSelectedPrompt(null);
                  setShowEditor(true);
                }}
                className="flex justify-center items-center gap-2 bg-indigo-600 hover:bg-indigo-700 shadow-lg px-6 py-3 rounded-lg w-full font-medium text-white transition-colors"
                style={{ fontSize: '16px' }}
              >
                <Plus className="w-5 h-5" />
                New Prompt
              </button>
            </div>
          </div>

          {/* Filters Row */}
          <div className="gap-4 grid grid-cols-1 lg:grid-cols-2 mb-6">
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="bg-white px-4 py-3 border-2 border-gray-200 focus:border-indigo-500 rounded-lg focus:outline-none text-gray-800"
              style={{ fontSize: '16px' }}
            >
              <option value="">All Categories</option>
              {categories.map(category => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>

            <div className="flex flex-wrap gap-2">
              {tags.map(tag => (
                <button
                  key={tag.id}
                  onClick={() => {
                    if (selectedTags.includes(tag.id)) {
                      setSelectedTags(selectedTags.filter(t => t !== tag.id));
                    } else {
                      setSelectedTags([...selectedTags, tag.id]);
                    }
                  }}
                  className={`px-4 py-2 rounded-full font-medium transition-all ${
                    selectedTags.includes(tag.id)
                      ? 'bg-indigo-600 text-white shadow-lg'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                  style={{ fontSize: '14px' }}
                >
                  {tag.name}
                </button>
              ))}
            </div>
          </div>

          {/* Management Buttons Row */}
          <div className="flex flex-wrap gap-3 pt-4 border-gray-200 border-t">
            <button
              onClick={() => setShowCategoryManager(true)}
              className="bg-gray-100 hover:bg-gray-200 px-6 py-2 rounded-lg font-medium text-gray-700 transition-colors"
              style={{ fontSize: '14px' }}
            >
              📁 Manage Categories
            </button>
            <button
              onClick={() => setShowTagManager(true)}
              className="bg-gray-100 hover:bg-gray-200 px-6 py-2 rounded-lg font-medium text-gray-700 transition-colors"
              style={{ fontSize: '14px' }}
            >
              🏷️ Manage Tags
            </button>
          </div>
        </div>        {/* Stats Dashboard */}
        {!isLoading && (
          <div className="mb-8">
            <StatsDashboard 
              prompts={prompts}
              categories={categories}
              tags={tags}
              hideRecentActivity={!!searchTerm || !!selectedCategory || selectedTags.length > 0}
            />
          </div>
        )}

        {/* Content Area */}        <div className="min-h-96">
          {isLoading ? (
            <div className="flex justify-center items-center py-20">
              <LoadingSpinner size="large" text="Loading your prompt workspace..." />
            </div>
          ) : filteredPrompts.length === 0 && (searchTerm || selectedCategory || selectedTags.length > 0) ? (
            <div className="flex flex-col justify-center items-center py-20 text-center">
              <Search className="mb-4 w-16 h-16 text-gray-300" />
              <h3 className="mb-2 font-semibold text-gray-600 text-xl">No prompts found</h3>
              <p className="mb-4 text-gray-500">
                {searchTerm ? (
                  <>No prompts match your search for &quot;{searchTerm}&quot;</>
                ) : selectedCategory ? (
                  <>No prompts found in the selected category</>
                ) : (
                  <>No prompts match your selected filters</>
                )}
              </p>
              <button
                onClick={() => {
                  setSearchTerm('');
                  setSelectedCategory('');
                  setSelectedTags([]);
                }}
                className="bg-indigo-500 hover:bg-indigo-600 px-4 py-2 rounded-lg text-white transition-colors"
              >
                Clear filters
              </button>
            </div>
          ) : filteredPrompts.length === 0 ? (
            <div className="flex flex-col justify-center items-center py-20 text-center">
              <Plus className="mb-4 w-16 h-16 text-gray-300" />
              <h3 className="mb-2 font-semibold text-gray-600 text-xl">No prompts yet</h3>
              <p className="mb-4 text-gray-500">
                Create your first prompt template to get started
              </p>
              <button
                onClick={() => {
                  setSelectedPrompt(null);
                  setShowEditor(true);
                }}
                className="bg-indigo-500 hover:bg-indigo-600 px-6 py-3 rounded-lg text-white transition-colors"
              >
                <Plus className="inline mr-2 w-5 h-5" />
                Create First Prompt
              </button>
            </div>
          ) : (
            <PromptList
              prompts={filteredPrompts}
              categories={categories}
              tags={tags}
              onEdit={(prompt) => {
                setSelectedPrompt(prompt);
                setShowEditor(true);
              }}
              onDelete={handleDeletePrompt}
              onExecute={(prompt) => {
                setSelectedPrompt(prompt);
                setShowEditor(true);
              }}
            />
          )}
        </div>

        {/* Help Button */}
        <button
          onClick={() => setShowWelcomeTour(true)}
          className="right-6 bottom-6 z-50 fixed bg-white hover:bg-gray-50 shadow-lg hover:shadow-xl p-4 rounded-full text-indigo-600 transition-all"
          title="Show Help"
        >
          <HelpCircle className="w-6 h-6" />
        </button>

        {/* Modals */}
        {showCategoryManager && (
          <CategoryManager
            categories={categories}
            onSave={async (category: Category) => {
              try {
                if (category.id) {
                  await categoryAPI.update(category.id, category);
                } else {
                  await categoryAPI.create(category);
                }
                await loadData();
                toast.success('Category saved successfully');
              } catch {
                toast.error('Failed to save category');
              }
            }}
            onDelete={async (id: string) => {
              try {
                await categoryAPI.delete(id);
                await loadData();
                toast.success('Category deleted successfully');
              } catch {
                toast.error('Failed to delete category');
              }
            }}
            onClose={() => setShowCategoryManager(false)}
          />
        )}

        {showTagManager && (
          <TagManager
            tags={tags}
            onSave={async (tag: Tag) => {
              try {
                if (tag.id) {
                  await tagAPI.update(tag.id, tag);
                } else {
                  await tagAPI.create(tag);
                }
                await loadData();
                toast.success('Tag saved successfully');
              } catch {
                toast.error('Failed to save tag');
              }
            }}
            onDelete={async (id: string) => {
              try {
                await tagAPI.delete(id);
                await loadData();
                toast.success('Tag deleted successfully');
              } catch {
                toast.error('Failed to delete tag');
              }
            }}
            onClose={() => setShowTagManager(false)}
          />
        )}

        {showWelcomeTour && (
          <WelcomeTour
            onClose={() => {
              setShowWelcomeTour(false);
              localStorage.setItem('hasSeenTour', 'true');
            }}
          />
        )}
      </div>
    </div>
  );
}
