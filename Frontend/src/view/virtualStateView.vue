<template>
	<h2>Overview</h2>
	<virtual-asset-list :assets="inventory" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import VirtualAssetList from '@/component/virtualAssetList.vue';

@Options({
	name: 'VirtualStateView',
	components: {
		VirtualAssetList,
	},
})
export default class VirtualStateView extends Vue {
	@State('virtualState')
	virtualState!: VirtualStateDto;

	get inventory() {
		return this.virtualState.inventory;
	}
}
</script>
