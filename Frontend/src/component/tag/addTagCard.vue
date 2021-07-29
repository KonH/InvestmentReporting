<template>
	<span>
		<button v-show="!isExpanded" class="btn btn-sm btn-outline-primary" @click="expand">+</button>
		<select v-show="isExpanded && !isNew" ref="tag" class="mr-1 custom-select-sm" @input="onTagSelect">
			<option v-for="tag in tagValues" :key="tag" :value="tag">{{ tag }}</option>
		</select>
		<input v-show="isExpanded && isNew" ref="newTag" type="text" class="mr-1" />
		<button v-show="isExpanded" :class="buttonClass" @click="add">+</button>
		<button v-show="isExpanded" class="btn btn-sm btn-outline-primary" @click="close">-</button>
	</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop, Ref } from 'vue-property-decorator';
import { AssetTagSetDto } from '@/api/meta';
import { Action } from 'vuex-class';
import Backend from '@/service/backend';
import Progress from '@/utils/progress';

@Options({
	name: 'AddTagCard',
})
export default class AddTagCard extends Vue {
	@Action('fetchTagState')
	fetchTagState!: () => void;

	@Ref('tag')
	tagSelect!: HTMLSelectElement;

	@Ref('newTag')
	tagInput!: HTMLInputElement;

	@Prop()
	asset!: AssetTagSetDto;

	@Prop()
	tags!: string[];

	isExpanded = false;

	newMarker = 'New...';

	isNew = false;

	isInProgress = false;

	expand() {
		this.isExpanded = true;
	}

	get buttonClass() {
		return Progress.getClass(this, 'btn btn-sm btn-primary mr-1');
	}

	async add() {
		await Progress.wrap(this, this.addApply);
	}

	get tagValues() {
		return [''].concat(this.tags).concat([this.newMarker]);
	}

	onTagSelect() {
		if (this.tagSelect.value == this.newMarker) {
			this.isNew = true;
		}
	}

	async addApply() {
		const tagValue = this.isNew ? this.tagInput.value : this.tagSelect.value;
		await Backend.meta().tag.postTag({
			asset: this.asset.isin,
			tag: tagValue,
		});
		this.close();
		this.fetchTagState();
	}

	close() {
		this.isExpanded = false;
		this.isNew = false;
		this.tagInput.value = '';
		this.tagSelect.value = '';
	}
}
</script>
