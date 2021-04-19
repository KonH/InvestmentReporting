<template>
	<span>
		<button v-show="!isExpanded" class="btn btn-sm btn-outline-primary" @click="expand">+</button>
		<input v-show="isExpanded" ref="tag" type="text" class="mr-1" />
		<button v-show="isExpanded" class="btn btn-sm btn-primary mr-1" @click="add">+</button>
		<button v-show="isExpanded" class="btn btn-sm btn-outline-primary" @click="close">-</button>
	</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop, Ref } from 'vue-property-decorator';
import { AssetTagSetDto } from '@/api/meta';
import { Action } from 'vuex-class';
import Backend from '@/service/backend';

@Options({
	name: 'AddTagCard',
})
export default class AddTagCard extends Vue {
	@Action('fetchTagState')
	fetchTagState!: () => void;

	@Ref('tag')
	tagInput!: HTMLInputElement;

	@Prop()
	asset!: AssetTagSetDto;

	isExpanded = false;

	expand() {
		this.isExpanded = true;
	}

	async add() {
		await Backend.meta().tag.postTag({
			asset: this.asset.isin,
			tag: this.tagInput.value,
		});
		this.close();
		this.fetchTagState();
	}

	close() {
		this.isExpanded = false;
		this.tagInput.value = '';
	}
}
</script>
