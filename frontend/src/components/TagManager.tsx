'use client';

import { useState } from 'react';
import { X, Plus, Edit, Trash2, Save } from 'lucide-react';
import { Tag } from '@/types';

interface TagManagerProps {
  tags: Tag[];
  onSave: (tag: Tag) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  onClose: () => void;
}

export default function TagManager({
  tags,
  onSave,
  onDelete,
  onClose,
}: TagManagerProps) {
  const [editingTag, setEditingTag] = useState<Tag | null>(null);
  const [isCreating, setIsCreating] = useState(false);  const [formData, setFormData] = useState({
    name: '',
    description: '',
    color: '#3B82F6',
  });
  const handleCreate = () => {
    setIsCreating(true);
    setEditingTag(null);
    setFormData({
      name: '',
      description: '',
      color: '#3B82F6',
    });
  };
  const handleEdit = (tag: Tag) => {
    setEditingTag(tag);
    setIsCreating(false);
    setFormData({
      name: tag.name,
      description: tag.description,
      color: tag.color,
    });
  };

  const handleSave = async () => {
    if (!formData.name.trim()) {
      return;
    }    const tagToSave: Tag = {
      id: editingTag?.id || '',
      name: formData.name,
      description: formData.description,
      color: formData.color,
    };    await onSave(tagToSave);
    setEditingTag(null);
    setIsCreating(false);
    setFormData({
      name: '',
      description: '',
      color: '#3B82F6',
    });
  };
  const handleCancel = () => {
    setEditingTag(null);
    setIsCreating(false);
    setFormData({
      name: '',
      description: '',
      color: '#3B82F6',
    });
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this tag?')) {
      await onDelete(id);
    }
  };

  const colorOptions = [
    '#3B82F6', // blue
    '#10B981', // green
    '#F59E0B', // yellow
    '#EF4444', // red
    '#8B5CF6', // purple
    '#06B6D4', // cyan
    '#F97316', // orange
    '#84CC16', // lime
    '#EC4899', // pink
    '#6B7280', // gray
  ];

  return (
    <div className="z-50 fixed inset-0 flex justify-center items-center bg-black bg-opacity-50">
      <div className="bg-white shadow-xl mx-4 rounded-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex justify-between items-center p-6 border-gray-200 border-b">
          <h2 className="font-semibold text-gray-900 text-xl">Manage Tags</h2>
          <button
            onClick={onClose}
            className="p-2 rounded-md text-gray-400 hover:text-gray-600"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {/* Create/Edit Form */}
          {(isCreating || editingTag) && (
            <div className="bg-gray-50 mb-6 p-4 rounded-lg">
              <h3 className="mb-4 font-medium text-gray-900 text-lg">
                {editingTag ? 'Edit Tag' : 'Create New Tag'}
              </h3>
              <div className="space-y-4">                <div>
                  <label className="block mb-2 font-medium text-gray-700 text-sm">
                    Name *
                  </label>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                    placeholder="Tag name"
                    autoFocus
                  />
                </div>
                <div>
                  <label className="block mb-2 font-medium text-gray-700 text-sm">
                    Description
                  </label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    className="px-3 py-2 border border-gray-300 focus:border-transparent rounded-md focus:ring-2 focus:ring-blue-500 w-full"
                    placeholder="Brief description of what this tag represents"
                    rows={2}
                  />
                </div>
                <div>
                  <label className="block mb-2 font-medium text-gray-700 text-sm">
                    Color
                  </label>
                  <div className="flex flex-wrap gap-2">
                    {colorOptions.map((color) => (
                      <button
                        key={color}
                        onClick={() => setFormData({ ...formData, color })}
                        className={`w-8 h-8 rounded-full border-2 ${
                          formData.color === color ? 'border-gray-800' : 'border-gray-300'
                        }`}
                        style={{ backgroundColor: color }}
                      />
                    ))}
                  </div>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={handleSave}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-md text-white"
                  >
                    <Save className="w-4 h-4" />
                    Save
                  </button>
                  <button
                    onClick={handleCancel}
                    className="hover:bg-gray-50 px-4 py-2 border border-gray-300 rounded-md text-gray-600"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Create Button */}
          {!isCreating && !editingTag && (
            <div className="mb-6">
              <button
                onClick={handleCreate}
                className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-md text-white"
              >
                <Plus className="w-4 h-4" />
                Create Tag
              </button>
            </div>
          )}          {/* Tags List */}
          <div className="gap-3 grid grid-cols-1">
            {tags.map((tag) => (
              <div
                key={tag.id}
                className="flex justify-between items-start hover:bg-gray-50 p-4 border border-gray-200 rounded-lg"
              >
                <div className="flex flex-1 items-start gap-3">
                  <div
                    className="flex-shrink-0 mt-1 rounded-full w-4 h-4"
                    style={{ backgroundColor: tag.color }}
                  />
                  <div className="flex-1">
                    <span className="block font-medium text-gray-900">{tag.name}</span>
                    {tag.description && (
                      <p className="mt-1 text-gray-600 text-sm">{tag.description}</p>
                    )}
                  </div>
                </div>
                <div className="flex gap-1 ml-3">
                  <button
                    onClick={() => handleEdit(tag)}
                    className="hover:bg-blue-50 p-1 rounded-md text-blue-600"
                  >
                    <Edit className="w-3 h-3" />
                  </button>
                  <button
                    onClick={() => handleDelete(tag.id)}
                    className="hover:bg-red-50 p-1 rounded-md text-red-600"
                  >
                    <Trash2 className="w-3 h-3" />
                  </button>
                </div>
              </div>
            ))}            {tags.length === 0 && (
              <div className="py-8 text-gray-500 text-center">
                No tags yet. Create your first tag to get started.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
