'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import { Save, X, Play, Download, Plus, Minus } from 'lucide-react';
import { PromptTemplate, Category, Tag, PromptVariable } from '@/types';
import * as yaml from 'js-yaml';
import toast from 'react-hot-toast';

interface PromptEditorProps {
  prompt?: PromptTemplate | null;
  categories: Category[];
  tags: Tag[];
  onSave: (prompt: PromptTemplate) => Promise<void>;
  onCancel: () => void;
  onExecute: (prompt: PromptTemplate, variables: Record<string, unknown>) => Promise<string | null>;
}

export default function PromptEditor({
  prompt,
  categories,
  tags,
  onSave,
  onCancel,
  onExecute,
}: PromptEditorProps) {
  const [formData, setFormData] = useState<Partial<PromptTemplate>>({
    name: '',
    description: '',
    content: '',
    categoryId: '',
    tags: [],
    variables: [],
  });  const [executionVariables, setExecutionVariables] = useState<Record<string, unknown>>({});
  const [executionResult, setExecutionResult] = useState<string | null>(null);
  const [isExecuting, setIsExecuting] = useState(false);
  const [activeTab, setActiveTab] = useState<'editor' | 'variables' | 'test'>('editor');
  const debounceTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (prompt) {
      setFormData(prompt);
      // Initialize execution variables with default values
      const initVars: Record<string, unknown> = {};
      prompt.variables.forEach(variable => {
        initVars[variable.name] = variable.defaultValue || '';
      });
      setExecutionVariables(initVars);
    }  }, [prompt]);

  // Cleanup timeout on component unmount
  useEffect(() => {
    return () => {
      if (debounceTimeoutRef.current) {
        clearTimeout(debounceTimeoutRef.current);
      }
    };
  }, []);  // Function to detect variables in content and automatically add them
  const detectAndAddVariables = useCallback((content: string) => {
    // Find all variables in the format {{variableName}}
    // This pattern requires exactly {{ and }} with non-brace content in between
    const variableMatches = content.match(/\{\{[^{}]+\}\}/g);
    const contentVariableNames = variableMatches 
      ? variableMatches.map(match => match.slice(2, -2).trim()).filter(Boolean)
      : [];

    const existingVariables = formData.variables || [];
    const existingVariableNames = existingVariables.map(v => v.name);
    
    // Find variables to add (in content but not in variables list)
    const variablesToAdd = contentVariableNames.filter(name => 
      !existingVariableNames.includes(name)
    );
    
    // Find variables to remove (in variables list but not in content)
    const variablesToRemove = existingVariableNames.filter(name => 
      !contentVariableNames.includes(name)
    );

    let updatedVariables = [...existingVariables];
    let hasChanges = false;

    // Add new variables
    variablesToAdd.forEach(variableName => {
      const newVariable: PromptVariable = {
        name: variableName,
        description: `Auto-detected variable: ${variableName}`,
        type: 'string',
        required: false,
        defaultValue: '',
      };
      updatedVariables.push(newVariable);
      hasChanges = true;
    });

    // Remove variables that are no longer in content
    if (variablesToRemove.length > 0) {
      updatedVariables = updatedVariables.filter(variable => 
        !variablesToRemove.includes(variable.name)
      );
      hasChanges = true;
    }

    if (hasChanges) {
      setFormData(prev => ({ ...prev, variables: updatedVariables }));
      
      // Show appropriate toast messages
      if (variablesToAdd.length > 0) {
        toast.success(`Added ${variablesToAdd.length} new variable(s): ${variablesToAdd.join(', ')}`);
      }      if (variablesToRemove.length > 0) {
        toast.success(`Removed ${variablesToRemove.length} unused variable(s): ${variablesToRemove.join(', ')}`);
      }
    }
  }, [formData.variables]);

  // Debounced version of variable detection to prevent typing interference
  const debouncedDetectAndAddVariables = useCallback((content: string) => {
    // Clear existing timeout
    if (debounceTimeoutRef.current) {
      clearTimeout(debounceTimeoutRef.current);
    }

    // Set new timeout for 500ms after user stops typing
    debounceTimeoutRef.current = setTimeout(() => {
      detectAndAddVariables(content);
    }, 500);
  }, [detectAndAddVariables]);

  const handleSave = async () => {
    if (!formData.name || !formData.content) {
      toast.error('Name and content are required');
      return;
    }

    const yamlTemplate = generateYamlTemplate();
    const promptToSave: PromptTemplate = {
      id: formData.id || '',
      name: formData.name,
      description: formData.description || '',
      content: formData.content,
      yamlTemplate,
      categoryId: formData.categoryId,
      tags: formData.tags || [],
      variables: formData.variables || [],
      createdAt: formData.createdAt || new Date(),
      updatedAt: new Date(),
    };

    await onSave(promptToSave);
  };

  const generateYamlTemplate = () => {
    const template = {
      name: formData.name,
      description: formData.description,
      content: formData.content,
      variables: formData.variables,
      category: categories.find(c => c.id === formData.categoryId)?.name,
      tags: formData.tags?.map(tagId => tags.find(t => t.id === tagId)?.name).filter(Boolean),
    };
    return yaml.dump(template);
  };

  const handleExecute = async () => {
    if (!formData.name || !formData.content) {
      toast.error('Please save the prompt first');
      return;
    }

    setIsExecuting(true);
    try {
      const promptToExecute: PromptTemplate = {
        id: formData.id || 'temp',
        name: formData.name,
        description: formData.description || '',
        content: formData.content,
        yamlTemplate: generateYamlTemplate(),
        categoryId: formData.categoryId,
        tags: formData.tags || [],
        variables: formData.variables || [],
        createdAt: formData.createdAt || new Date(),
        updatedAt: new Date(),
      };

      const result = await onExecute(promptToExecute, executionVariables);
      setExecutionResult(result);
      setActiveTab('test');
    } catch (error) {
      console.error('Error executing prompt:', error);
    } finally {
      setIsExecuting(false);
    }
  };

  const addVariable = () => {
    const newVariable: PromptVariable = {
      name: '',
      description: '',
      type: 'string',
      required: false,
      defaultValue: '',
    };
    setFormData({
      ...formData,
      variables: [...(formData.variables || []), newVariable],
    });
  };  const updateVariable = (index: number, variable: PromptVariable) => {
    const variables = [...(formData.variables || [])];
    const oldVariable = variables[index];
    variables[index] = variable;
    
    // If variable name changed, update all instances in the prompt content
    if (oldVariable.name !== variable.name && oldVariable.name && variable.name) {
      const updatedContent = (formData.content || '').replace(
        new RegExp(`\\{\\{${oldVariable.name.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\}\\}`, 'g'),
        `{{${variable.name}}}`
      );
      setFormData({ ...formData, variables, content: updatedContent });
      toast.success(`Updated variable name from "${oldVariable.name}" to "${variable.name}" in prompt content`);
    } else {
      setFormData({ ...formData, variables });
    }
  };
  const removeVariable = (index: number) => {
    const variables = [...(formData.variables || [])];
    const variableToRemove = variables[index];
    variables.splice(index, 1);
    
    // Remove all instances of this variable from the prompt content
    if (variableToRemove.name) {
      let updatedContent = (formData.content || '').replace(
        new RegExp(`\\{\\{${variableToRemove.name.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\}\\}`, 'g'),
        ''
      );
      
      // Clean up any double spaces or spaces at line boundaries that might result
      updatedContent = updatedContent
        .replace(/\s{2,}/g, ' ') // Replace multiple spaces with single space
        .replace(/^\s+|\s+$/gm, '') // Trim spaces at start/end of lines
        .replace(/\n\s*\n\s*\n/g, '\n\n'); // Replace multiple empty lines with double newline
      
      setFormData({ ...formData, variables, content: updatedContent });
      toast.success(`Removed variable "${variableToRemove.name}" from prompt content`);
    } else {
      setFormData({ ...formData, variables });
    }
  };

  const downloadYaml = () => {
    const yamlContent = generateYamlTemplate();
    const blob = new Blob([yamlContent], { type: 'text/yaml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${formData.name || 'prompt'}.yaml`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };
  return (
    <div className="bg-gradient-to-br from-slate-50 to-blue-50 min-h-screen">
      <div className="mx-auto px-6 py-8 max-w-6xl container">
        {/* Header */}
        <div className="bg-white shadow-lg mb-8 p-6 border border-gray-200 rounded-xl">
          <div className="flex justify-between items-center mb-6">
            <div>
              <h1 className="mb-2 font-bold text-gray-900 text-3xl">
                {prompt ? '✏️ Edit Prompt' : '✨ Create New Prompt'}
              </h1>
              <p className="text-gray-600">
                {prompt ? 'Modify your prompt template' : 'Build a new prompt template for your AI workflows'}
              </p>
            </div>
            <div className="flex gap-3">
              <button
                onClick={downloadYaml}
                className="flex items-center gap-2 hover:bg-gray-100 hover:shadow-md px-4 py-2 border border-gray-300 rounded-lg font-medium text-gray-700 hover:text-gray-900 transition-all duration-200"
              >
                <Download className="w-4 h-4" />
                Export YAML
              </button>
              <button
                onClick={handleExecute}
                disabled={isExecuting}
                className="flex items-center gap-2 bg-gradient-to-r from-green-600 hover:from-green-700 to-green-700 hover:to-green-800 disabled:opacity-50 shadow-lg hover:shadow-xl px-4 py-2 rounded-lg font-medium text-white transition-all duration-200 disabled:cursor-not-allowed"
              >
                <Play className="w-4 h-4" />
                {isExecuting ? 'Executing...' : '🧪 Test'}
              </button>
              <button
                onClick={handleSave}
                className="flex items-center gap-2 bg-gradient-to-r from-blue-600 hover:from-blue-700 to-blue-700 hover:to-blue-800 shadow-lg hover:shadow-xl px-5 py-2 rounded-lg font-medium text-white transition-all hover:-translate-y-0.5 duration-200 transform"
              >
                <Save className="w-4 h-4" />
                Save
              </button>
              <button
                onClick={onCancel}
                className="flex items-center gap-2 hover:bg-gray-100 hover:shadow-md px-4 py-2 border border-gray-300 rounded-lg font-medium text-gray-700 hover:text-gray-900 transition-all duration-200"
              >
                <X className="w-4 h-4" />
                Cancel
              </button>
            </div>
          </div>          {/* Tabs */}
          <div className="border-gray-200 border-b">
            <nav className="flex space-x-8 -mb-px">
              {['editor', 'variables', 'test'].map((tab) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab as typeof activeTab)}
                  className={`py-3 px-1 border-b-2 font-medium text-sm capitalize transition-all duration-200 ${
                    activeTab === tab
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  {tab === 'editor' && '📝 '}
                  {tab === 'variables' && '🔧 '}
                  {tab === 'test' && '🧪 '}
                  {tab.charAt(0).toUpperCase() + tab.slice(1)}
                </button>
              ))}
            </nav>
          </div>
        </div>

        {/* Tab Content */}
        <div className="bg-white shadow-sm border rounded-lg">
          {activeTab === 'editor' && (
            <div className="space-y-6 p-6">
              {/* Basic Info */}
              <div className="gap-6 grid grid-cols-1 md:grid-cols-2">
                <div>
                  <label className="block mb-2 font-medium text-gray-700 text-sm">
                    Name *
                  </label>
                  <input
                    type="text"
                    value={formData.name || ''}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                    placeholder="Enter prompt name"
                  />
                </div>
                <div>
                  <label className="block mb-2 font-medium text-gray-700 text-sm">
                    Category
                  </label>
                  <select
                    value={formData.categoryId || ''}
                    onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                    className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                  >
                    <option value="">No Category</option>
                    {categories.map(category => (
                      <option key={category.id} value={category.id}>
                        {category.name}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block mb-2 font-medium text-gray-700 text-sm">
                  Description
                </label>
                <textarea
                  value={formData.description || ''}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  rows={3}
                  className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                  placeholder="Enter prompt description"
                />
              </div>

              <div>
                <label className="block mb-2 font-medium text-gray-700 text-sm">
                  Prompt Content *
                </label>                <textarea
                  value={formData.content || ''}
                  onChange={(e) => {
                    const newContent = e.target.value;
                    setFormData({ ...formData, content: newContent });
                    // Use debounced variable detection to prevent typing interference
                    debouncedDetectAndAddVariables(newContent);
                  }}
                  rows={12}
                  className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full font-mono text-sm"
                  placeholder="Enter your prompt content here. Use {{variableName}} for variables."
                />
              </div>

              {/* Tags */}
              <div>
                <label className="block mb-2 font-medium text-gray-700 text-sm">
                  Tags
                </label>
                <div className="flex flex-wrap gap-2">
                  {tags.map(tag => (
                    <label key={tag.id} className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        checked={formData.tags?.includes(tag.id) || false}
                        onChange={(e) => {
                          const newTags = formData.tags || [];
                          if (e.target.checked) {
                            setFormData({ ...formData, tags: [...newTags, tag.id] });
                          } else {
                            setFormData({ ...formData, tags: newTags.filter(t => t !== tag.id) });
                          }
                        }}
                        className="border-gray-300 rounded focus:ring-blue-500 text-blue-600"
                      />
                      <span className="text-gray-700 text-sm">{tag.name}</span>
                    </label>
                  ))}
                </div>
              </div>
            </div>
          )}

          {activeTab === 'variables' && (
            <div className="p-6">
              <div className="flex justify-between items-center mb-4">
                <h3 className="font-medium text-gray-900 text-lg">Variables</h3>
                <button
                  onClick={addVariable}
                  className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-md text-white"
                >
                  <Plus className="w-4 h-4" />
                  Add Variable
                </button>
              </div>

              <div className="space-y-4">
                {(formData.variables || []).map((variable, index) => (
                  <div key={index} className="p-4 border border-gray-200 rounded-lg">
                    <div className="gap-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 mb-4">
                      <div>
                        <label className="block mb-1 font-medium text-gray-700 text-sm">
                          Name
                        </label>
                        <input
                          type="text"
                          value={variable.name}
                          onChange={(e) => updateVariable(index, { ...variable, name: e.target.value })}
                          className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                          placeholder="Variable name"
                        />
                      </div>
                      <div>
                        <label className="block mb-1 font-medium text-gray-700 text-sm">
                          Type
                        </label>
                        <select
                          value={variable.type}
                          onChange={(e) => updateVariable(index, { ...variable, type: e.target.value as 'string' | 'number' | 'boolean' })}
                          className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                        >
                          <option value="string">String</option>
                          <option value="number">Number</option>
                          <option value="boolean">Boolean</option>
                        </select>
                      </div>
                      <div>
                        <label className="block mb-1 font-medium text-gray-700 text-sm">
                          Default Value
                        </label>
                        <input
                          type="text"
                          value={variable.defaultValue || ''}
                          onChange={(e) => updateVariable(index, { ...variable, defaultValue: e.target.value })}
                          className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                          placeholder="Default value"
                        />
                      </div>
                    </div>
                    <div className="flex justify-between items-center">
                      <div>
                        <label className="block mb-1 font-medium text-gray-700 text-sm">
                          Description
                        </label>
                        <input
                          type="text"
                          value={variable.description}
                          onChange={(e) => updateVariable(index, { ...variable, description: e.target.value })}
                          className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                          placeholder="Variable description"
                        />
                      </div>
                      <div className="flex items-center gap-4 ml-4">
                        <label className="flex items-center">
                          <input
                            type="checkbox"
                            checked={variable.required}
                            onChange={(e) => updateVariable(index, { ...variable, required: e.target.checked })}
                            className="mr-2 border-gray-300 rounded focus:ring-blue-500 text-blue-600"
                          />
                          Required
                        </label>
                        <button
                          onClick={() => removeVariable(index)}
                          className="hover:bg-red-50 p-2 rounded-md text-red-600"
                        >
                          <Minus className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {activeTab === 'test' && (
            <div className="p-6">
              <div className="gap-6 grid grid-cols-1 lg:grid-cols-2">
                {/* Input Variables */}
                <div>
                  <h3 className="mb-4 font-medium text-gray-900 text-lg">Test Variables</h3>
                  <div className="space-y-4">
                    {(formData.variables || []).map((variable) => (
                      <div key={variable.name}>
                        <label className="block mb-1 font-medium text-gray-700 text-sm">
                          {variable.name} {variable.required && <span className="text-red-500">*</span>}
                        </label>
                        <input
                          type={variable.type === 'number' ? 'number' : 'text'}
                          value={executionVariables[variable.name] as string || ''}
                          onChange={(e) => setExecutionVariables({
                            ...executionVariables,
                            [variable.name]: variable.type === 'number' ? Number(e.target.value) : e.target.value
                          })}
                          className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                          placeholder={variable.description || variable.name}
                        />
                      </div>
                    ))}
                  </div>
                </div>

                {/* Execution Result */}
                <div>
                  <h3 className="mb-4 font-medium text-gray-900 text-lg">Result</h3>
                  <div className="bg-gray-50 p-4 border border-gray-200 rounded-md min-h-[200px]">
                    {executionResult ? (
                      <pre className="text-gray-800 text-sm whitespace-pre-wrap">
                        {executionResult}
                      </pre>
                    ) : (
                      <p className="text-gray-500 italic">
                        Click &quot;Test&quot; to execute the prompt with the current variables.
                      </p>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
