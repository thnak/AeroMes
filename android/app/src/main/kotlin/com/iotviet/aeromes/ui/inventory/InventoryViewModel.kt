package com.iotviet.aeromes.ui.inventory

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.InventoryItem
import com.iotviet.aeromes.domain.repository.InventoryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class InventoryUiState(
    val query: String = "",
    val items: List<InventoryItem> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
)

@HiltViewModel
class InventoryViewModel @Inject constructor(
    private val inventoryRepository: InventoryRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(InventoryUiState())
    val uiState = _uiState.asStateFlow()

    fun onQueryChange(value: String) = _uiState.update { it.copy(query = value) }

    fun search() {
        val query = _uiState.value.query.trim()
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }
            // Simple heuristic: if it looks like a lot number (contains letters+digits mix) search both ways
            val productCode = if (query.all { it.isLetter() || it == '-' }) query else null
            val lotNumber = if (!query.all { it.isLetter() || it == '-' }) query else null
            inventoryRepository.getInventory(
                productCode = productCode.takeIf { it != null && query.isNotBlank() } ?: query.takeIf { it.isNotBlank() },
                lotNumber = lotNumber.takeIf { it != null && query.isNotBlank() }
            )
                .onSuccess { items -> _uiState.update { it.copy(items = items, isLoading = false) } }
                .onFailure { e -> _uiState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }
}
